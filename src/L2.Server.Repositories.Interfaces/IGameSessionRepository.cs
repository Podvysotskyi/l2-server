namespace L2.Server.Repositories.Interfaces;

public interface IGameSessionRepository
{
    Task<GameSessionRecord?> RedeemAsync(
        byte[] ticketTokenHash,
        byte[] accessTokenHash,
        string gameVersion,
        string gameServer,
        Guid sessionId,
        DateTimeOffset now,
        CancellationToken cancellationToken);
    Task<GameSessionRecord?> FindActiveAsync(
        byte[] accessTokenHash,
        string gameVersion,
        string gameServer,
        DateTimeOffset now,
        DateTimeOffset idleCutoff,
        CancellationToken cancellationToken);
    Task SelectCharacterAsync(
        Guid sessionId,
        Guid characterId,
        DateTimeOffset now,
        CancellationToken cancellationToken);
    Task ClearCharacterAsync(Guid sessionId, Guid characterId, CancellationToken cancellationToken);
    Task RevokeAsync(Guid sessionId, DateTimeOffset now, CancellationToken cancellationToken);
}
