using System.Net.WebSockets;
using System.Text.Json;
using L2.PlayerIdentity;
using L2.PlayerCharacters;
using L2.Shared;
using Microsoft.Extensions.Options;

namespace L2.GameServer.Sessions;

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
        GameSessionTicketRepository tickets,
        PlayerCharacterService characters,
        IOptions<GameSessionOptions> options,
        IConfiguration configuration,
        ServiceIdentity identity,
        ILoggerFactory loggerFactory)
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

        var logger = loggerFactory.CreateLogger("GameSession");
        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        using var authenticationTimeout = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        authenticationTimeout.CancelAfter(TimeSpan.FromSeconds(options.Value.AuthenticationTimeoutSeconds));

        SessionAuthenticate? handshake;
        try
        {
            handshake = await ReceiveHandshakeAsync(socket, authenticationTimeout.Token);
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

        if (handshake is null ||
            handshake.Type != "session.authenticate" ||
            string.IsNullOrWhiteSpace(handshake.Ticket) ||
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

        AuthenticatedAccount? account;
        try
        {
            account = await tickets.ConsumeAsync(handshake.Ticket, context.RequestAborted);
        }
        catch (PlayerIdentityPersistenceException exception)
        {
            logger.LogError(exception, "Game session ticket redemption failed because persistence is unavailable");
            await RejectAsync(socket, "service_unavailable", "Game session service is unavailable.");
            return;
        }

        if (account is null)
        {
            logger.LogWarning("Rejected an invalid game session ticket");
            await RejectAsync(socket, "invalid_ticket", "Game session ticket was rejected.");
            return;
        }

        logger.LogInformation("Authenticated game connection for account {AccountId}", account.AccountId);
        await SendAsync(socket, new
        {
            type = "session.ready",
            accountId = account.AccountId,
            username = account.Username,
            protocolVersion = options.Value.ProtocolVersion,
            serverBuild = FoundationExtensions.BuildVersion(),
            service = identity.Name
        }, context.RequestAborted);

        while (socket.State == WebSocketState.Open && !context.RequestAborted.IsCancellationRequested)
        {
            JsonElement? message;
            try
            {
                message = await ReceiveMessageAsync(socket, context.RequestAborted);
            }
            catch (JsonException)
            {
                await SendAsync(socket, new { type = "character.error", code = "invalid_message" }, context.RequestAborted);
                continue;
            }
            if (message is null)
            {
                if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                {
                    await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None);
                }
                return;
            }

            await HandleCharacterMessageAsync(
                socket,
                account.AccountId,
                message.Value,
                characters,
                context.RequestAborted);
        }
    }

    private static async Task<SessionAuthenticate?> ReceiveHandshakeAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[MaximumHandshakeBytes];
        var result = await socket.ReceiveAsync(buffer, cancellationToken);
        if (result.MessageType != WebSocketMessageType.Text || !result.EndOfMessage || result.Count == MaximumHandshakeBytes)
        {
            await socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Authentication message is too large.", CancellationToken.None);
            return null;
        }

        return JsonSerializer.Deserialize<SessionAuthenticate>(buffer.AsSpan(0, result.Count), JsonOptions);
    }

    private static async Task<JsonElement?> ReceiveMessageAsync(
        WebSocket socket,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[MaximumMessageBytes];
        var result = await socket.ReceiveAsync(buffer, cancellationToken);
        if (result.MessageType == WebSocketMessageType.Close) return null;
        if (result.MessageType != WebSocketMessageType.Text || !result.EndOfMessage || result.Count == buffer.Length)
        {
            await socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Gameplay message is too large.", CancellationToken.None);
            return null;
        }

        using var document = JsonDocument.Parse(buffer.AsMemory(0, result.Count));
        return document.RootElement.Clone();
    }

    private static async Task HandleCharacterMessageAsync(
        WebSocket socket,
        Guid accountId,
        JsonElement message,
        PlayerCharacterService characters,
        CancellationToken cancellationToken)
    {
        if (!message.TryGetProperty("type", out var typeValue))
        {
            await SendAsync(socket, new { type = "character.error", code = "invalid_message" }, cancellationToken);
            return;
        }

        var type = typeValue.GetString();
        switch (type)
        {
            case "character.list":
                await SendAsync(socket, new
                {
                    type = "character.list",
                    characters = await characters.ListAsync(accountId, cancellationToken)
                }, cancellationToken);
                break;
            case "character.creation-options":
                await SendAsync(socket, new
                {
                    type = "character.creation-options",
                    options = await characters.GetCreationOptionsAsync(cancellationToken)
                }, cancellationToken);
                break;
            case "character.create":
                var request = message.Deserialize<CharacterCreateMessage>(JsonOptions);
                if (request is null || string.IsNullOrWhiteSpace(request.Name))
                {
                    await SendAsync(socket, new { type = "character.error", code = "invalid_message" }, cancellationToken);
                    break;
                }
                await SendResultAsync(socket, "character.created", await characters.CreateAsync(accountId,
                    new CharacterCreationRequest(request.Name, request.ClassId, request.RaceId, request.SexId,
                        request.FaceId, request.HairStyleId, request.HairColorId), cancellationToken), cancellationToken);
                break;
            case "character.select":
                await HandleIdMessageAsync(socket, message, "character.selected",
                    (id, token) => characters.SelectAsync(accountId, id, token), cancellationToken);
                break;
            case "character.delete":
                await HandleIdMessageAsync(socket, message, "character.deletion-scheduled",
                    (id, token) => characters.ScheduleDeletionAsync(accountId, id, token), cancellationToken);
                break;
            case "character.restore":
                await HandleIdMessageAsync(socket, message, "character.restored",
                    (id, token) => characters.RestoreAsync(accountId, id, token), cancellationToken);
                break;
            default:
                await SendAsync(socket, new { type = "character.error", code = "unsupported_message" }, cancellationToken);
                break;
        }
    }

    private static async Task HandleIdMessageAsync(
        WebSocket socket,
        JsonElement message,
        string responseType,
        Func<Guid, CancellationToken, Task<CharacterOperationResult>> operation,
        CancellationToken cancellationToken)
    {
        if (!message.TryGetProperty("characterId", out var idValue) || !idValue.TryGetGuid(out var id))
        {
            await SendAsync(socket, new { type = "character.error", code = "invalid_message" }, cancellationToken);
            return;
        }
        await SendResultAsync(socket, responseType, await operation(id, cancellationToken), cancellationToken);
    }

    private static Task SendResultAsync(WebSocket socket, string type, CharacterOperationResult result,
        CancellationToken cancellationToken) => result.Succeeded
        ? SendAsync(socket, new { type, character = result.Character }, cancellationToken)
        : SendAsync(socket, new { type = "character.error", code = result.ErrorCode }, cancellationToken);

    private static async Task RejectAsync(WebSocket socket, string code, string reason)
    {
        if (socket.State != WebSocketState.Open)
        {
            return;
        }

        await SendAsync(socket, new { type = "session.rejected", code }, CancellationToken.None);
        await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, reason, CancellationToken.None);
    }

    private static Task SendAsync(WebSocket socket, object message, CancellationToken cancellationToken) =>
        socket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions), WebSocketMessageType.Text, true, cancellationToken);

    private static bool OriginAllowed(HttpContext context, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        var origin = context.Request.Headers.Origin.ToString();
        return allowedOrigins.Length > 0 && allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }
}
