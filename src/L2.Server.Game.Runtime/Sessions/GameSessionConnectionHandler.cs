using System.Net.WebSockets;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using L2.Server.Configurations;
using L2.Server.Contracts;
using L2.Server.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace L2.Server.Game.Sessions;

public sealed class GameSessionConnectionHandler(
    IGameSessionService sessions,
    IPlayerCharacterService characters,
    IOptions<GameConnectionOptions> options,
    IConfiguration configuration,
    ServiceIdentity identity)
{
    private const int MaximumHandshakeBytes = 4 * 1024;
    private const int MaximumMessageBytes = 32 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task HandleAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }
        if (!OriginAllowed(context))
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
            handshake = await ReceiveAsync<GameSessionAuthentication>(
                socket,
                MaximumHandshakeBytes,
                timeout.Token);
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

        if (!Valid(handshake))
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

        var selected = await characters.SelectAsync(
            session.AccountId,
            session.GameVersion,
            session.GameServer,
            characterId,
            context.RequestAborted);
        if (selected is not { Succeeded: true, Character: not null })
        {
            await RejectAsync(socket, "character_unavailable", "Selected character is unavailable.");
            return;
        }

        await SendAsync(socket, new GameSessionReady(
            "session.ready",
            session.AccountId,
            session.Username,
            session.GameVersion,
            session.GameServer,
            selected.Character,
            options.Value.ProtocolVersion,
            identity.BuildVersion,
            identity.Name), context.RequestAborted);

        var buffer = new byte[MaximumMessageBytes];
        while (socket.State == WebSocketState.Open && !context.RequestAborted.IsCancellationRequested)
        {
            var result = await socket.ReceiveAsync(buffer, context.RequestAborted);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseOutputAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Client closed",
                    CancellationToken.None);
                return;
            }
            if (result.MessageType != WebSocketMessageType.Text ||
                !result.EndOfMessage ||
                result.Count == buffer.Length)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.MessageTooBig,
                    "Gameplay message is too large.",
                    CancellationToken.None);
                return;
            }
            await SendAsync(socket, new { type = "game.error", code = "unsupported_message" },
                context.RequestAborted);
        }
    }

    private static bool Valid([NotNullWhen(true)] GameSessionAuthentication? handshake) => handshake is not null &&
        handshake.Type == "session.authenticate" &&
        !string.IsNullOrWhiteSpace(handshake.AccessToken) &&
        !string.IsNullOrWhiteSpace(handshake.ClientBuild) &&
        !string.IsNullOrWhiteSpace(handshake.AssetRelease) &&
        !string.IsNullOrWhiteSpace(handshake.GameDataRelease);

    private static async Task<T?> ReceiveAsync<T>(
        WebSocket socket,
        int maximumBytes,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[maximumBytes];
        var result = await socket.ReceiveAsync(buffer, cancellationToken);
        if (result.MessageType != WebSocketMessageType.Text ||
            !result.EndOfMessage ||
            result.Count == buffer.Length)
        {
            await socket.CloseAsync(
                WebSocketCloseStatus.MessageTooBig,
                "Authentication message is too large.",
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

    private static Task SendAsync(
        WebSocket socket,
        object message,
        CancellationToken cancellationToken) => socket.SendAsync(
            JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions),
            WebSocketMessageType.Text,
            true,
            cancellationToken);

    private bool OriginAllowed(HttpContext context)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        var origin = context.Request.Headers.Origin.ToString();
        return allowedOrigins.Length > 0 &&
            allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }
}
