using L2.Server.Contracts;
using L2.Server.Repositories.Interfaces;
using L2.Server.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace L2.Server.Services.Tests;

public sealed class PlayerCharacterServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateAsync_rejects_invalid_names_before_persistence()
    {
        var repository = new StubRepository();
        var service = CreateService(repository);

        var result = await service.CreateAsync(Guid.NewGuid(),
            new CharacterCreationRequest("!", 0, 0, 0, 0, 0, 0));

        Assert.False(result.Succeeded);
        Assert.Equal("invalid_name", result.ErrorCode);
        Assert.Null(repository.Created);
    }

    [Fact]
    public async Task CreateAsync_normalizes_valid_data_and_delegates()
    {
        var repository = new StubRepository
        {
            CreationOptions = new CharacterCreationOptions(0,
            [
                new RootClassOption(1, "Fighter", false,
                [
                    new RaceOption(2, "Human",
                    [
                        new SexOption(3, "Male",
                            [new AppearanceOption(4, "Face")],
                            [new AppearanceOption(5, "Hair")],
                            [new AppearanceOption(6, "Color")])
                    ])
                ])
            ]),
            CreateResult = new CharacterMutationResult(true)
        };
        var service = CreateService(repository);

        var result = await service.CreateAsync(Guid.NewGuid(),
            new CharacterCreationRequest(" Hero ", 1, 2, 3, 4, 5, 6));

        Assert.True(result.Succeeded);
        Assert.NotNull(repository.Created);
        Assert.Equal("Hero", repository.Created.Name);
        Assert.Equal("HERO", repository.Created.NormalizedName);
        Assert.Equal(Now, repository.Created.CreatedAt);
    }

    [Fact]
    public async Task ScheduleDeletionAsync_calculates_deadline_in_the_service()
    {
        var repository = new StubRepository();
        var service = CreateService(repository);

        await service.ScheduleDeletionAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(Now, repository.DeletionNow);
        Assert.Equal(Now.AddDays(7), repository.DeleteAfter);
    }

    private static PlayerCharacterService CreateService(StubRepository repository) => new(
        repository,
        Options.Create(new PlayerCharacterOptions
        {
            MaximumCharactersPerAccount = 7,
            MinimumNameLength = 2,
            MaximumNameLength = 16,
            DeletionDelayDays = 7
        }),
        new FixedTimeProvider(Now));

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class StubRepository : IPlayerCharacterRepository
    {
        public CharacterCreationOptions CreationOptions { get; init; } = new(0, []);
        public CharacterMutationResult CreateResult { get; init; } = new(false);
        public CharacterCreationData? Created { get; private set; }
        public DateTimeOffset? DeleteAfter { get; private set; }
        public DateTimeOffset? DeletionNow { get; private set; }

        public Task<int> CleanupExpiredAsync(
            DateTimeOffset now,
            CancellationToken cancellationToken = default) => Task.FromResult(0);

        public Task<IReadOnlyList<PlayerCharacterSummary>> ListAsync(
            Guid accountId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PlayerCharacterSummary>>([]);

        public Task<CharacterCreationOptions> GetCreationOptionsAsync(
            CancellationToken cancellationToken = default) => Task.FromResult(CreationOptions);

        public Task<CharacterMutationResult> CreateAsync(
            CharacterCreationData character,
            CancellationToken cancellationToken = default)
        {
            Created = character;
            return Task.FromResult(CreateResult);
        }

        public Task<CharacterMutationResult> SelectAsync(
            Guid accountId,
            Guid characterId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new CharacterMutationResult(false));

        public Task<CharacterMutationResult> ScheduleDeletionAsync(
            Guid accountId,
            Guid characterId,
            DateTimeOffset deleteAfter,
            DateTimeOffset now,
            CancellationToken cancellationToken = default)
        {
            DeleteAfter = deleteAfter;
            DeletionNow = now;
            return Task.FromResult(new CharacterMutationResult(true));
        }

        public Task<CharacterMutationResult> RestoreAsync(
            Guid accountId,
            Guid characterId,
            DateTimeOffset now,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new CharacterMutationResult(false));
    }
}
