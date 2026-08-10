using L2.Server.Contracts;
using L2.Server.Repositories.Interfaces;
using L2.Server.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace L2.Server.Services.Tests;

public sealed class GameSessionServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExchangeAsync_hashes_tokens_and_maps_repository_state()
    {
        var accountId = Guid.NewGuid();
        var repository = new StubGameSessionRepository
        {
            Redeemed = new GameSessionRecord(Guid.NewGuid(), accountId, "Player", null, Now.AddHours(1))
        };
        var service = new GameSessionService(
            repository,
            new StubCharacterService(),
            Options.Create(new GameSessionOptions { IdleTimeoutMinutes = 30 }),
            new FixedTimeProvider(Now));

        var issue = await service.ExchangeAsync("ticket", CancellationToken.None);

        Assert.NotNull(issue);
        Assert.Equal(accountId, issue.Session.AccountId);
        Assert.Equal(1800, issue.IdleTimeoutSeconds);
        Assert.Equal(32, repository.TicketHash?.Length);
        Assert.Equal(32, repository.AccessHash?.Length);
        Assert.Equal(Now, repository.RedeemedAt);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class StubGameSessionRepository : IGameSessionRepository
    {
        public GameSessionRecord? Redeemed { get; init; }
        public byte[]? TicketHash { get; private set; }
        public byte[]? AccessHash { get; private set; }
        public DateTimeOffset? RedeemedAt { get; private set; }

        public Task<GameSessionRecord?> RedeemAsync(
            byte[] ticketTokenHash,
            byte[] accessTokenHash,
            Guid sessionId,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            TicketHash = ticketTokenHash;
            AccessHash = accessTokenHash;
            RedeemedAt = now;
            return Task.FromResult(Redeemed);
        }

        public Task<GameSessionRecord?> FindActiveAsync(
            byte[] accessTokenHash,
            DateTimeOffset now,
            DateTimeOffset idleCutoff,
            CancellationToken cancellationToken) => Task.FromResult<GameSessionRecord?>(null);

        public Task SelectCharacterAsync(
            Guid sessionId,
            Guid characterId,
            DateTimeOffset now,
            CancellationToken cancellationToken) => Task.CompletedTask;

        public Task ClearCharacterAsync(
            Guid sessionId,
            Guid characterId,
            CancellationToken cancellationToken) => Task.CompletedTask;

        public Task RevokeAsync(
            Guid sessionId,
            DateTimeOffset now,
            CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class StubCharacterService : IPlayerCharacterService
    {
        public Task<IReadOnlyList<PlayerCharacterSummary>> ListAsync(
            Guid accountId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PlayerCharacterSummary>>([]);

        public Task<CharacterCreationOptions> GetCreationOptionsAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new CharacterCreationOptions(0, []));

        public Task<CharacterOperationResult> CreateAsync(
            Guid accountId,
            CharacterCreationRequest request,
            CancellationToken cancellationToken = default) => Task.FromResult(new CharacterOperationResult(false));

        public Task<CharacterOperationResult> SelectAsync(
            Guid accountId,
            Guid characterId,
            CancellationToken cancellationToken = default) => Task.FromResult(new CharacterOperationResult(false));

        public Task<CharacterOperationResult> ScheduleDeletionAsync(
            Guid accountId,
            Guid characterId,
            CancellationToken cancellationToken = default) => Task.FromResult(new CharacterOperationResult(false));

        public Task<CharacterOperationResult> RestoreAsync(
            Guid accountId,
            Guid characterId,
            CancellationToken cancellationToken = default) => Task.FromResult(new CharacterOperationResult(false));
    }
}
