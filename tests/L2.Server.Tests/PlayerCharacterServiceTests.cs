using L2.GameContent;
using L2.GameContent.Entities;
using L2.GameContent.Identifiers;
using L2.PlayerCharacters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace L2.Foundation.Tests;

[Collection(PostgreSqlIntegrationCollection.Name)]
public sealed class PlayerCharacterServiceTests : IAsyncLifetime
{
    private readonly PostgreSqlIntegrationFixture postgres;
    private PostgreSqlDatabaseLease? database;
    private TestTimeProvider time = null!;
    private PlayerCharacterService service = null!;

    public PlayerCharacterServiceTests(PostgreSqlIntegrationFixture postgres) => this.postgres = postgres;

    public async Task InitializeAsync()
    {
        database = await postgres.CreateDatabaseAsync();
        var contentOptions = new DbContextOptionsBuilder<GameContentDbContext>()
            .UseNpgsql(database.ConnectionString, postgresOptions => postgresOptions.MigrationsHistoryTable(
                "__EFMigrationsHistory", GameContentDbContext.SchemaName)).Options;
        var characterOptions = new DbContextOptionsBuilder<PlayerCharactersDbContext>()
            .UseNpgsql(database.ConnectionString, postgresOptions => postgresOptions.MigrationsHistoryTable(
                "__EFMigrationsHistory", PlayerCharactersDbContext.SchemaName)).Options;
        var contentFactory = new TestContextFactory<GameContentDbContext>(() => new(contentOptions));
        var characterFactory = new TestContextFactory<PlayerCharactersDbContext>(() => new(characterOptions));
        await using (var content = contentFactory.CreateDbContext())
        {
            await content.Database.MigrateAsync();
            content.PlayerRaces.Add(new PlayerRace { Id = PlayerRaceId.Human, Name = "Human" });
            content.PlayerSexes.AddRange(
                new PlayerSex { Id = PlayerSexId.Male, Name = "Male" },
                new PlayerSex { Id = PlayerSexId.Female, Name = "Female" });
            content.PlayerClasses.AddRange(
                new PlayerClass
                {
                    Id = PlayerClassId.HumanFighter,
                    PlayerRaceId = PlayerRaceId.Human,
                    PlayerSexId = PlayerSexId.Male,
                    Name = "Human Fighter",
                    IsMage = false
                },
                new PlayerClass
                {
                    Id = PlayerClassId.HumanMystic,
                    PlayerRaceId = PlayerRaceId.Human,
                    PlayerSexId = PlayerSexId.Male,
                    Name = "Human Mystic",
                    IsMage = true
                });
            foreach (var id in Enumerable.Range(0, 3)) content.PlayerFaces.Add(new PlayerFace
            {
                Id = id,
                Name = $"Face {id}",
                PlayerRaceId = PlayerRaceId.Human,
                PlayerSexId = PlayerSexId.Male
            });
            foreach (var id in Enumerable.Range(0, 5)) content.PlayerHairStyles.Add(new PlayerHairStyle
            {
                Id = id,
                Name = $"Hair {id}",
                PlayerRaceId = PlayerRaceId.Human,
                PlayerSexId = PlayerSexId.Male
            });
            foreach (var id in Enumerable.Range(0, 4)) content.PlayerHairColors.Add(new PlayerHairColor
            {
                Id = id,
                Name = $"Color {id}",
                PlayerRaceId = PlayerRaceId.Human,
                PlayerSexId = PlayerSexId.Male
            });
            await content.SaveChangesAsync();
        }
        await using (var characters = characterFactory.CreateDbContext())
        {
            await characters.Database.MigrateAsync();
        }
        time = new TestTimeProvider(new DateTimeOffset(2026, 8, 9, 12, 0, 0, TimeSpan.Zero));
        service = new PlayerCharacterService(characterFactory, contentFactory,
            Options.Create(new PlayerCharacterOptions { MaximumCharactersPerAccount = 2 }), time);
    }

    public async Task DisposeAsync()
    {
        if (database is not null) await database.DisposeAsync();
    }

    [Fact]
    public async Task Creation_options_are_built_from_exact_root_variants_and_appearance_rows()
    {
        var options = await service.GetCreationOptionsAsync();
        Assert.Equal(2, options.MaximumCharacters);
        Assert.Equal(2, options.Classes.Count);
        Assert.False(Assert.Single(options.Classes, item => item.Id == 0).IsMage);
        var mystic = Assert.Single(options.Classes, item => item.Id == 10);
        Assert.True(mystic.IsMage);
        var sex = Assert.Single(Assert.Single(mystic.AllowedRaces).AllowedSexes);
        Assert.Equal(3, sex.Faces.Count);
        Assert.Equal(5, sex.HairStyles.Count);
        Assert.Equal(4, sex.HairColors.Count);
    }

    [Fact]
    public async Task Creation_validates_names_variants_appearances_and_case_insensitive_uniqueness()
    {
        var account = Guid.NewGuid();
        Assert.Equal("invalid_name", (await service.CreateAsync(account, Request("!"))).ErrorCode);
        Assert.Equal("invalid_class_variant", (await service.CreateAsync(account,
            Request("ValidName") with { SexId = 1 })).ErrorCode);
        Assert.Equal("invalid_appearance", (await service.CreateAsync(account,
            Request("ValidName") with { HairStyleId = 99 })).ErrorCode);
        Assert.True((await service.CreateAsync(account, Request("ValidName"))).Succeeded);
        var character = Assert.Single(await service.ListAsync(account));
        Assert.Equal(0, character.AccountSlot);
        Assert.Equal("name_taken", (await service.CreateAsync(Guid.NewGuid(), Request("validname"))).ErrorCode);
    }

    [Fact]
    public async Task Account_limit_is_enforced_under_concurrent_creation()
    {
        var account = Guid.NewGuid();
        var results = await Task.WhenAll(
            service.CreateAsync(account, Request("ConcurrentA")),
            service.CreateAsync(account, Request("ConcurrentB")));
        Assert.All(results, result => Assert.True(result.Succeeded));
        Assert.Equal("character_limit", (await service.CreateAsync(account, Request("ConcurrentC"))).ErrorCode);
        Assert.Equal(2, (await service.ListAsync(account)).Count);
    }

    [Fact]
    public async Task Selection_enforces_ownership_and_deletion_is_reversible_until_expiry()
    {
        var account = Guid.NewGuid();
        var created = await service.CreateAsync(account, Request("Lifecycle"));
        var id = created.Character!.Id;
        Assert.Equal("character_not_found", (await service.SelectAsync(Guid.NewGuid(), id)).ErrorCode);
        Assert.True((await service.SelectAsync(account, id)).Succeeded);
        var scheduled = await service.ScheduleDeletionAsync(account, id);
        Assert.Equal(time.GetUtcNow().AddDays(7), scheduled.Character!.DeleteAfter);
        Assert.Equal("character_pending_deletion", (await service.SelectAsync(account, id)).ErrorCode);
        Assert.True((await service.RestoreAsync(account, id)).Succeeded);
        await service.ScheduleDeletionAsync(account, id);
        time.Advance(TimeSpan.FromDays(8));
        Assert.Equal("deletion_expired", (await service.RestoreAsync(account, id)).ErrorCode);
        Assert.Equal(1, await service.CleanupExpiredAsync());
        Assert.Equal(0, await service.CleanupExpiredAsync());
        Assert.Empty(await service.ListAsync(account));
    }

    private static CharacterCreationRequest Request(string name) => new(
        name, (int)PlayerClassId.HumanFighter, (int)PlayerRaceId.Human,
        (int)PlayerSexId.Male, 0, 0, 0);

    private sealed class TestContextFactory<TContext>(Func<TContext> create) : IDbContextFactory<TContext>
        where TContext : DbContext
    {
        public TContext CreateDbContext() => create();
        public Task<TContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(create());
    }

    private sealed class TestTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
        public void Advance(TimeSpan duration) => utcNow += duration;
    }
}
