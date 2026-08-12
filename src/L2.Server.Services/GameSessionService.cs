using L2.Server.Contracts;
using L2.Server.Repositories.Interfaces;
using L2.Server.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace L2.Server.Services;

public sealed class GameSessionService(
    IGameSessionRepository repository,
    IPlayerCharacterService characters,
    IOptions<GameSessionOptions> options,
    TimeProvider timeProvider) : IGameSessionService
{
    private readonly GameSessionOptions options = options.Value;

    public async Task<GameSessionIssue?> ExchangeAsync(string ticket, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ticket)) return null;
        var accessToken = OpaqueToken.Create();
        var now = timeProvider.GetUtcNow();
        var record = await repository.RedeemAsync(
            OpaqueToken.Hash(ticket),
            OpaqueToken.Hash(accessToken),
            Guid.NewGuid(),
            now,
            cancellationToken);
        return record is null
            ? null
            : new GameSessionIssue(accessToken, ToState(record), checked(this.options.IdleTimeoutMinutes * 60));
    }

    public Task<GameSessionState?> AuthenticateAsync(string accessToken, CancellationToken cancellationToken) =>
        string.IsNullOrWhiteSpace(accessToken)
            ? Task.FromResult<GameSessionState?>(null)
            : FindActiveAsync(accessToken, cancellationToken);

    private async Task<GameSessionState?> FindActiveAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var record = await repository.FindActiveAsync(
            OpaqueToken.Hash(accessToken),
            now,
            now.AddMinutes(-options.IdleTimeoutMinutes),
            cancellationToken);
        return record is null ? null : ToState(record);
    }

    public async Task<CharacterOperationResult> SelectCharacterAsync(
        GameSessionState session,
        Guid characterId,
        CancellationToken cancellationToken)
    {
        var result = await characters.SelectAsync(
            session.AccountId,
            session.GameVersion,
            characterId,
            cancellationToken);
        if (result.Succeeded)
            await repository.SelectCharacterAsync(
                session.SessionId,
                characterId,
                timeProvider.GetUtcNow(),
                cancellationToken);
        return result;
    }

    public async Task<CharacterOperationResult> ScheduleDeletionAsync(
        GameSessionState session,
        Guid characterId,
        CancellationToken cancellationToken)
    {
        var result = await characters.ScheduleDeletionAsync(
            session.AccountId,
            session.GameVersion,
            characterId,
            cancellationToken);
        if (result.Succeeded)
            await repository.ClearCharacterAsync(session.SessionId, characterId, cancellationToken);
        return result;
    }

    public Task RevokeAsync(GameSessionState session, CancellationToken cancellationToken) =>
        repository.RevokeAsync(session.SessionId, timeProvider.GetUtcNow(), cancellationToken);

    private static GameSessionState ToState(GameSessionRecord record) => new(
        record.SessionId,
        record.AccountId,
        record.Username,
        record.GameVersion,
        record.SelectedCharacterId,
        record.ExpiresAt);
}
