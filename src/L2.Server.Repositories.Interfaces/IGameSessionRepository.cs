using L2.Server.Services.Interfaces;

namespace L2.Server.Repositories.Interfaces;

public interface IGameSessionRepository
{
    Task<GameSessionState?> RedeemAsync(
        string ticket,
        byte[] accessTokenHash,
        Guid sessionId,
        CancellationToken cancellationToken);
    Task<GameSessionState?> FindActiveAsync(
        byte[] accessTokenHash,
        DateTimeOffset idleCutoff,
        CancellationToken cancellationToken);
    Task SelectCharacterAsync(Guid sessionId, Guid characterId, CancellationToken cancellationToken);
    Task ClearCharacterAsync(Guid sessionId, Guid characterId, CancellationToken cancellationToken);
    Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken);
}
