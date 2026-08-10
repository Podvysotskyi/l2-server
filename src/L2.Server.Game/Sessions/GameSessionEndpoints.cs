using System.Net.WebSockets;
using System.Text.Json;
using L2.Server.Contracts;
using L2.Server.Services.Interfaces;
using L2.Server.Services;
using L2.Server.Configurations;
using Microsoft.Extensions.Options;

namespace L2.Server.Game.Sessions;

public static class GameSessionEndpoints
{
    private const int MaximumHandshakeBytes = 4 * 1024;
    private const int MaximumMessageBytes = 32 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static IEndpointRouteBuilder MapGameSessions(this IEndpointRouteBuilder endpoints)
    {
        endpoints.Map("/ws/game", HandleAsync);
        return endpoints;
    }

    private static async Task HandleAsync(
        HttpContext context,
        IGameSessionService sessions,
        IPlayerCharacterService characters,
        IOptions<GameSessionOptions> options,
        IConfiguration configuration,
        ServiceIdentity identity)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }
        if (!OriginAllowed(context, configuration))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.Value.AuthenticationTimeoutSeconds));

        GameSessionAuthentication? handshake;
        try
        {
            handshake = await ReceiveAsync<GameSessionAuthentication>(socket, MaximumHandshakeBytes, timeout.Token);
        }
        catch (OperationCanceledException)
        {
            await RejectAsync(socket, "invalid_handshake", "Authentication timed out.");
            return;
        }
        catch (JsonException)
        {
            await RejectAsync(socket, "invalid_handshake", "Authentication message was invalid.");
            return;
        }

        if (handshake is null || handshake.Type != "session.authenticate" ||
            string.IsNullOrWhiteSpace(handshake.AccessToken) ||
            string.IsNullOrWhiteSpace(handshake.ClientBuild) ||
            string.IsNullOrWhiteSpace(handshake.AssetRelease) ||
            string.IsNullOrWhiteSpace(handshake.GameDataRelease))
        {
            await RejectAsync(socket, "invalid_handshake", "Authentication message was invalid.");
            return;
        }
        if (handshake.ProtocolVersion != options.Value.ProtocolVersion)
        {
            await RejectAsync(socket, "incompatible_protocol", "Protocol version is not supported.");
            return;
        }

        var session = await sessions.AuthenticateAsync(handshake.AccessToken, context.RequestAborted);
        if (session is null)
        {
            await RejectAsync(socket, "invalid_session", "Game session was rejected.");
            return;
        }
        if (session.SelectedCharacterId is not { } characterId)
        {
            await RejectAsync(socket, "character_not_selected", "Select a character before connecting.");
            return;
        }

        var selected = await characters.SelectAsync(session.AccountId, characterId, context.RequestAborted);
        if (selected is not { Succeeded: true, Character: not null })
        {
            await RejectAsync(socket, "character_unavailable", "Selected character is unavailable.");
            return;
        }

        await SendAsync(socket, new GameSessionReady(
            "session.ready",
            session.AccountId,
            session.Username,
            selected.Character,
            options.Value.ProtocolVersion,
            FoundationExtensions.BuildVersion(),
            identity.Name), context.RequestAborted);

        var buffer = new byte[MaximumMessageBytes];
        while (socket.State == WebSocketState.Open && !context.RequestAborted.IsCancellationRequested)
        {
            var result = await socket.ReceiveAsync(buffer, context.RequestAborted);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None);
                return;
            }
            if (result.MessageType != WebSocketMessageType.Text || !result.EndOfMessage || result.Count == buffer.Length)
            {
                await socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Gameplay message is too large.",
                    CancellationToken.None);
                return;
            }
            await SendAsync(socket, new { type = "game.error", code = "unsupported_message" },
                context.RequestAborted);
        }
    }

    private static async Task<T?> ReceiveAsync<T>(WebSocket socket, int maximumBytes, CancellationToken cancellationToken)
    {
        var buffer = new byte[maximumBytes];
        var result = await socket.ReceiveAsync(buffer, cancellationToken);
        if (result.MessageType != WebSocketMessageType.Text || !result.EndOfMessage || result.Count == buffer.Length)
        {
            await socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Authentication message is too large.",
                CancellationToken.None);
            return default;
        }
        return JsonSerializer.Deserialize<T>(buffer.AsSpan(0, result.Count), JsonOptions);
    }

    private static async Task RejectAsync(WebSocket socket, string code, string reason)
    {
        if (socket.State != WebSocketState.Open) return;
        await SendAsync(socket, new GameSessionRejected("session.rejected", code), CancellationToken.None);
        await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, reason, CancellationToken.None);
    }

    private static Task SendAsync(WebSocket socket, object message, CancellationToken cancellationToken) =>
        socket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions),
            WebSocketMessageType.Text, true, cancellationToken);

    private static bool OriginAllowed(HttpContext context, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        var origin = context.Request.Headers.Origin.ToString();
        return allowedOrigins.Length > 0 && allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }
}
