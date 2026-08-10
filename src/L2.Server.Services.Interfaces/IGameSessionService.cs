using L2.Server.Contracts;

namespace L2.Server.Services.Interfaces;

public interface IGameSessionService
{
    Task<GameSessionIssue?> ExchangeAsync(string ticket, CancellationToken cancellationToken);
    Task<GameSessionState?> AuthenticateAsync(string accessToken, CancellationToken cancellationToken);
    Task<CharacterOperationResult> SelectCharacterAsync(
        GameSessionState session,
        Guid characterId,
        CancellationToken cancellationToken);
    Task<CharacterOperationResult> ScheduleDeletionAsync(
        GameSessionState session,
        Guid characterId,
        CancellationToken cancellationToken);
    Task RevokeAsync(GameSessionState session, CancellationToken cancellationToken);
}
