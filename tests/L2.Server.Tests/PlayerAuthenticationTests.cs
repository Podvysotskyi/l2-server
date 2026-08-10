using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using L2.LoginServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace L2.Foundation.Tests;

[Collection(PostgreSqlIntegrationCollection.Name)]
public sealed class PlayerAuthenticationTests : IAsyncLifetime
{
    private readonly PostgreSqlIntegrationFixture postgres;
    private PostgreSqlDatabaseLease? database;
    private AuthFactory? factory;

    public PlayerAuthenticationTests(PostgreSqlIntegrationFixture postgres) => this.postgres = postgres;

    public async Task InitializeAsync()
    {
        database = await postgres.CreateDatabaseAsync();
        factory = new AuthFactory(database.ConnectionString);
    }

    public async Task DisposeAsync()
    {
        if (factory is not null)
        {
            await factory.DisposeAsync();
        }

        if (database is not null) await database.DisposeAsync();
    }

    [Fact]
    public async Task Registration_requires_a_separate_login_and_logout_works_end_to_end()
    {
        using var client = factory!.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });
        var username = $"Player_{Guid.NewGuid():N}"[..20];
        var email = $"{username}@example.com";
        const string password = "Password1";

        var csrf = await GetCsrfAsync(client);
        var mismatched = await PostAsync(client, "/api/auth/register", new
        {
            username,
            email,
            password,
            passwordConfirmation = "different horse battery value"
        }, csrf);
        Assert.Equal(HttpStatusCode.BadRequest, mismatched.Response.StatusCode);

        var registration = await PostAsync(client, "/api/auth/register", new
        {
            username,
            email,
            password,
            passwordConfirmation = password
        }, csrf);
        Assert.Equal(HttpStatusCode.Created, registration.Response.StatusCode);
        Assert.False(registration.Response.Headers.TryGetValues("Set-Cookie", out _));
        var createdAccount = await registration.Response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(username, createdAccount.GetProperty("username").GetString());
        Assert.Equal(email, createdAccount.GetProperty("email").GetString());

        using var unsignedRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/session");
        unsignedRequest.Headers.Add("Cookie", csrf.Cookie);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(unsignedRequest)).StatusCode);

        var duplicate = await PostAsync(client, "/api/auth/register", new
        {
            username = $"Other_{Guid.NewGuid():N}"[..20],
            email = email.ToUpperInvariant(),
            password,
            passwordConfirmation = password
        }, csrf);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.Response.StatusCode);

        var usernameLogin = await PostAsync(client, "/api/auth/login", new { username, password }, csrf);
        Assert.Equal(HttpStatusCode.BadRequest, usernameLogin.Response.StatusCode);

        var invalid = await PostAsync(client, "/api/auth/login", new { email, password = "incorrect password value" }, csrf);
        Assert.Equal(HttpStatusCode.Unauthorized, invalid.Response.StatusCode);

        var login = await PostAsync(client, "/api/auth/login", new { email, password }, csrf);
        Assert.Equal(HttpStatusCode.OK, login.Response.StatusCode);
        var sessionCookie = SessionCookie(login.Response);

        using var currentRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/session");
        currentRequest.Headers.Add("Cookie", sessionCookie);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(currentRequest)).StatusCode);

        var logoutCsrf = csrf with { Cookie = $"{csrf.Cookie}; {sessionCookie}" };
        var logout = await PostAsync(client, "/api/auth/logout", null, logoutCsrf);
        Assert.Equal(HttpStatusCode.NoContent, logout.Response.StatusCode);

        using var loggedOutRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/session");
        loggedOutRequest.Headers.Add("Cookie", sessionCookie);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(loggedOutRequest)).StatusCode);
    }

    [Fact]
    public async Task Mutations_require_an_antiforgery_token()
    {
        using var client = factory!.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "player@example.com",
            password = "Password1"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<CsrfContext> GetCsrfAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/auth/csrf");
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        return new CsrfContext(payload.GetProperty("token").GetString()!, Cookie(response, "l2.csrf"));
    }

    private static async Task<(HttpResponseMessage Response, string? Cookie)> PostAsync(
        HttpClient client,
        string path,
        object? body,
        CsrfContext csrf)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Add("X-CSRF-TOKEN", csrf.Token);
        request.Headers.Add("Cookie", csrf.Cookie);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        var response = await client.SendAsync(request);
        return (response, response.Headers.TryGetValues("Set-Cookie", out _) ? response.Headers.GetValues("Set-Cookie").First() : null);
    }

    private static string SessionCookie(HttpResponseMessage response) => Cookie(response, "l2.player_session");

    private static string Cookie(HttpResponseMessage response, string name) => response.Headers
        .GetValues("Set-Cookie")
        .Select(value => value.Split(';', 2)[0])
        .Single(value => value.StartsWith($"{name}=", StringComparison.Ordinal));

    private sealed record CsrfContext(string Token, string Cookie);

    private sealed class AuthFactory(string connectionString) : WebApplicationFactory<LoginServerMarker>
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
}
