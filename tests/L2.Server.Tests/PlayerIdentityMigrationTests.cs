using L2.PlayerIdentity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace L2.Foundation.Tests;

[Collection(PostgreSqlIntegrationCollection.Name)]
public sealed class PlayerIdentityMigrationTests : IAsyncLifetime
{
    private readonly PostgreSqlIntegrationFixture postgres;
    private PostgreSqlDatabaseLease? database;

    public PlayerIdentityMigrationTests(PostgreSqlIntegrationFixture postgres) => this.postgres = postgres;

    public async Task InitializeAsync() => database = await postgres.CreateDatabaseAsync();

    public async Task DisposeAsync()
    {
        if (database is not null) await database.DisposeAsync();
    }

    [Fact]
    public async Task Initial_migration_creates_the_complete_identity_schema()
    {
        await using var context = CreateContext();
        await context.Database.MigrateAsync();

        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
        var expectedTables = new[]
        {
            "account_credentials",
            "account_login_history",
            "account_sessions",
            "accounts",
            "game_session_tickets"
        };
        var tables = await context.Database
            .SqlQueryRaw<string>(
                "SELECT table_name AS \"Value\" FROM information_schema.tables " +
                "WHERE table_schema = 'public' AND table_name <> '__EFMigrationsHistory' ORDER BY table_name")
            .ToListAsync();
        Assert.Equal(expectedTables, tables);

        var expectedIndexes = new[]
        {
            "ix_account_login_history_account_time",
            "ix_account_sessions_account_active",
            "ix_account_sessions_token_hash",
            "ix_accounts_normalized_email",
            "ix_accounts_normalized_username",
            "ix_game_session_tickets_account_session_id",
            "ix_game_session_tickets_pending_expiry",
            "ix_game_session_tickets_token_hash"
        };
        var indexes = await context.Database
            .SqlQueryRaw<string>(
                "SELECT indexname AS \"Value\" FROM pg_indexes WHERE schemaname = 'public' " +
                "AND indexname LIKE 'ix_%' ORDER BY indexname")
            .ToListAsync();
        Assert.Equal(expectedIndexes, indexes);
    }

    [Fact]
    public async Task Email_login_migration_preserves_existing_accounts_with_unique_placeholders()
    {
        await using var context = CreateContext();
        var migrator = context.Database.GetService<IMigrator>();
        await migrator.MigrateAsync("20260804201518_InitialPlayerIdentity");
        var accountId = Guid.NewGuid();
        const string username = "Legacy_Player";
        const string normalizedUsername = "LEGACY_PLAYER";
        var createdAt = DateTimeOffset.UtcNow;
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO accounts (id, username, normalized_username, created_at) VALUES ({accountId}, {username}, {normalizedUsername}, {createdAt})");

        await migrator.MigrateAsync();

        var account = await context.Accounts.AsNoTracking().SingleAsync(candidate => candidate.Id == accountId);
        Assert.Equal("legacy_player@legacy.invalid", account.Email);
        Assert.Equal("LEGACY_PLAYER@LEGACY.INVALID", account.NormalizedEmail);
    }

    private PlayerIdentityDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PlayerIdentityDbContext>()
            .UseNpgsql(database!.ConnectionString)
            .Options;
        return new PlayerIdentityDbContext(options);
    }
}
