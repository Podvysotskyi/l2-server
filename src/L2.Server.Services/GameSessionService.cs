using L2.Server.Contracts;
using L2.Server.Contracts.Security;
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
        var accessToken = GameSessionToken.Create();
        var session = await repository.RedeemAsync(
            ticket,
            GameSessionToken.Hash(accessToken),
            Guid.NewGuid(),
            cancellationToken);
        return session is null
            ? null
            : new GameSessionIssue(accessToken, session, checked(this.options.IdleTimeoutMinutes * 60));
    }

    public Task<GameSessionState?> AuthenticateAsync(string accessToken, CancellationToken cancellationToken) =>
        string.IsNullOrWhiteSpace(accessToken)
            ? Task.FromResult<GameSessionState?>(null)
            : repository.FindActiveAsync(
                GameSessionToken.Hash(accessToken),
                timeProvider.GetUtcNow().AddMinutes(-options.IdleTimeoutMinutes),
                cancellationToken);

    public async Task<CharacterOperationResult> SelectCharacterAsync(
        GameSessionState session,
        Guid characterId,
        CancellationToken cancellationToken)
    {
        var result = await characters.SelectAsync(session.AccountId, characterId, cancellationToken);
        if (result.Succeeded)
            await repository.SelectCharacterAsync(session.SessionId, characterId, cancellationToken);
        return result;
    }

    public async Task<CharacterOperationResult> ScheduleDeletionAsync(
        GameSessionState session,
        Guid characterId,
        CancellationToken cancellationToken)
    {
        var result = await characters.ScheduleDeletionAsync(session.AccountId, characterId, cancellationToken);
        if (result.Succeeded)
            await repository.ClearCharacterAsync(session.SessionId, characterId, cancellationToken);
        return result;
    }

    public Task RevokeAsync(GameSessionState session, CancellationToken cancellationToken) =>
        repository.RevokeAsync(session.SessionId, cancellationToken);
}
