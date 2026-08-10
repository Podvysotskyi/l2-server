using System.Net;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using L2.GameServer;
using L2.LoginServer;
using L2.PlayerIdentity;
using L2.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace L2.Foundation.Tests;

[Collection(PostgreSqlIntegrationCollection.Name)]
public sealed class GameSessionHandoffTests : IAsyncLifetime
{
    private const string AllowedOrigin = "http://localhost:13000";
    private readonly PostgreSqlIntegrationFixture postgres;
    private PostgreSqlDatabaseLease? database;
    private LoginFactory? loginFactory;
    private GameFactory? gameFactory;

    public GameSessionHandoffTests(PostgreSqlIntegrationFixture postgres) => this.postgres = postgres;

    public async Task InitializeAsync()
    {
        database = await postgres.CreateDatabaseAsync();
        loginFactory = new LoginFactory(database.ConnectionString);
        gameFactory = new GameFactory(database.ConnectionString);
    }

    public async Task DisposeAsync()
    {
        if (gameFactory is not null) await gameFactory.DisposeAsync();
        if (loginFactory is not null) await loginFactory.DisposeAsync();
        if (database is not null) await database.DisposeAsync();
    }

    [Fact]
    public async Task Ticket_is_hashed_single_use_and_bound_to_an_active_login_session()
    {
        using var loginClient = loginFactory!.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });
        var username = $"Player_{Guid.NewGuid():N}"[..20];
        var auth = await RegisterAsync(loginClient, username);

        var firstTicket = await IssueTicketAsync(loginClient, auth);
        await AssertTicketIsHashedAsync(firstTicket.Ticket);
        var ready = await AuthenticateAsync(firstTicket.Ticket);
        Assert.Equal("session.ready", ready.GetProperty("type").GetString());
        Assert.Equal(username, ready.GetProperty("username").GetString());

        var replay = await AuthenticateAsync(firstTicket.Ticket);
        Assert.Equal("session.rejected", replay.GetProperty("type").GetString());
        Assert.Equal("invalid_ticket", replay.GetProperty("code").GetString());

        var expiredTicket = await IssueTicketAsync(loginClient, auth);
        await using (var context = CreateIdentityContext())
        {
            var hash = GameSessionTicketToken.Hash(expiredTicket.Ticket);
            var storedTicket = await context.GameSessionTickets.SingleAsync(candidate =>
                candidate.TokenHash.SequenceEqual(hash));
            storedTicket.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1);
            await context.SaveChangesAsync();
        }

        var expired = await AuthenticateAsync(expiredTicket.Ticket);
        Assert.Equal("invalid_ticket", expired.GetProperty("code").GetString());

        var revokedTicket = await IssueTicketAsync(loginClient, auth);
        var logout = await PostAsync(loginClient, "/api/auth/logout", null, auth);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        var revoked = await AuthenticateAsync(revokedTicket.Ticket);
        Assert.Equal("invalid_ticket", revoked.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Concurrent_redemption_allows_exactly_one_game_connection()
    {
        using var loginClient = loginFactory!.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });
        var auth = await RegisterAsync(loginClient, $"Player_{Guid.NewGuid():N}"[..20]);
        var ticket = await IssueTicketAsync(loginClient, auth);

        var responses = await Task.WhenAll(
            AuthenticateAsync(ticket.Ticket),
            AuthenticateAsync(ticket.Ticket));

        Assert.Single(responses, response => response.GetProperty("type").GetString() == "session.ready");
        var rejected = Assert.Single(responses, response =>
            response.GetProperty("type").GetString() == "session.rejected");
        Assert.Equal("invalid_ticket", rejected.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Handoff_rejects_missing_authentication_and_disallowed_origins()
    {
        using var client = loginFactory!.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });
        var csrf = await GetCsrfAsync(client);
        var unauthorized = await PostAsync(client, "/api/auth/game-ticket", null, csrf);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        var auth = await RegisterAsync(client, $"Player_{Guid.NewGuid():N}"[..20]);
        using var missingCsrf = new HttpRequestMessage(HttpMethod.Post, "/api/auth/game-ticket");
        missingCsrf.Headers.Add("Cookie", auth.Cookie);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(missingCsrf)).StatusCode);

        var webSocketClient = gameFactory!.Server.CreateWebSocketClient();
        webSocketClient.ConfigureRequest = request => request.Headers["Origin"] = "https://not-allowed.example";
        await Assert.ThrowsAnyAsync<Exception>(() =>
            webSocketClient.ConnectAsync(new Uri("ws://localhost/ws/game"), CancellationToken.None));
    }

    [Fact]
    public async Task Handoff_rejects_malformed_oversized_and_late_authentication_messages()
    {
        using (var malformed = await ConnectAsync())
        {
            await malformed.SendAsync("{"u8.ToArray(), WebSocketMessageType.Text, true, CancellationToken.None);
            var response = await ReceiveAsync(malformed);
            Assert.Equal("invalid_handshake", response.GetProperty("code").GetString());
        }

        using (var oversized = await ConnectAsync())
        {
            await oversized.SendAsync(new byte[4096], WebSocketMessageType.Text, true, CancellationToken.None);
            var buffer = new byte[1];
            var close = await oversized.ReceiveAsync(buffer, CancellationToken.None);
            Assert.Equal(WebSocketMessageType.Close, close.MessageType);
            Assert.Equal(WebSocketCloseStatus.MessageTooBig, close.CloseStatus);
        }

        using (var late = await ConnectAsync())
        {
            var response = await ReceiveAsync(late);
            Assert.Equal("invalid_handshake", response.GetProperty("code").GetString());
        }
    }

    private async Task<JsonElement> AuthenticateAsync(string ticket)
    {
        using var socket = await ConnectAsync();
        await socket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new
        {
            type = "session.authenticate",
            ticket,
            protocolVersion = 1,
            clientBuild = "test",
            assetRelease = "development",
            gameDataRelease = "development"
        }), WebSocketMessageType.Text, true, CancellationToken.None);
        var payload = await ReceiveAsync(socket);
        if (payload.GetProperty("type").GetString() == "session.ready")
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);
        }

        return payload;
    }

    private async Task<WebSocket> ConnectAsync()
    {
        var client = gameFactory!.Server.CreateWebSocketClient();
        client.ConfigureRequest = request => request.Headers["Origin"] = AllowedOrigin;
        return await client.ConnectAsync(new Uri("ws://localhost/ws/game"), CancellationToken.None);
    }

    private static async Task<JsonElement> ReceiveAsync(WebSocket socket)
    {
        var buffer = new byte[1024];
        var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
        Assert.Equal(WebSocketMessageType.Text, result.MessageType);
        return JsonSerializer.Deserialize<JsonElement>(buffer.AsSpan(0, result.Count));
    }

    private async Task AssertTicketIsHashedAsync(string ticket)
    {
        await using var context = CreateIdentityContext();
        var expected = GameSessionTicketToken.Hash(ticket);
        var stored = await context.GameSessionTickets
            .Where(candidate => candidate.TokenHash.SequenceEqual(expected))
            .Select(candidate => candidate.TokenHash)
            .SingleAsync();
        Assert.Equal(GameSessionTicketToken.Hash(ticket), stored);
        Assert.NotEqual(Encoding.UTF8.GetBytes(ticket), stored);
    }

    private PlayerIdentityDbContext CreateIdentityContext()
    {
        var options = new DbContextOptionsBuilder<PlayerIdentityDbContext>()
            .UseNpgsql(database!.ConnectionString)
            .Options;
        return new PlayerIdentityDbContext(options);
    }

    private static async Task<AuthContext> RegisterAsync(HttpClient client, string username)
    {
        var email = $"{username.ToLowerInvariant()}@example.com";
        var csrf = await GetCsrfAsync(client);
        var response = await PostAsync(client, "/api/auth/register", new
        {
            username,
            email,
            password = "Password1",
            passwordConfirmation = "Password1"
        }, csrf);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var login = await PostAsync(client, "/api/auth/login", new
        {
            email,
            password = "Password1"
        }, csrf);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        return csrf with { Cookie = $"{csrf.Cookie}; {Cookie(login, "l2.player_session")}" };
    }

    private static async Task<TicketResponse> IssueTicketAsync(HttpClient client, AuthContext auth)
    {
        var response = await PostAsync(client, "/api/auth/game-ticket", null, auth);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<TicketResponse>())!;
    }

    private static async Task<AuthContext> GetCsrfAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/auth/csrf");
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        return new AuthContext(payload.GetProperty("token").GetString()!, Cookie(response, "l2.csrf"));
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string path, object? body, AuthContext auth)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Add("X-CSRF-TOKEN", auth.Token);
        request.Headers.Add("Cookie", auth.Cookie);
        if (body is not null) request.Content = JsonContent.Create(body);
        return await client.SendAsync(request);
    }

    private static string Cookie(HttpResponseMessage response, string name) => response.Headers
        .GetValues("Set-Cookie")
        .Select(value => value.Split(';', 2)[0])
        .Single(value => value.StartsWith($"{name}=", StringComparison.Ordinal));

    private sealed record AuthContext(string Token, string Cookie);

    private sealed record TicketResponse(string Ticket, DateTimeOffset ExpiresAt);

    private sealed class LoginFactory(string connectionString) : WebApplicationFactory<LoginServerMarker>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:PostgreSql", connectionString);
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Dependencies:PostgreSqlRequired"] = "false",
                    ["Authentication:RunPlayerIdentityMigrations"] = "true"
                }));
        }
    }

    private sealed class GameFactory(string connectionString) : WebApplicationFactory<GameServerMarker>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:PostgreSql", connectionString);
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Dependencies:PostgreSqlRequired"] = "false",
                    ["Cors:AllowedOrigins:0"] = AllowedOrigin,
                    ["GameSession:AuthenticationTimeoutSeconds"] = "1"
                }));
        }
    }
}
