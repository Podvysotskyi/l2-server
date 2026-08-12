using L2.Server.Context.Entities;
using Microsoft.EntityFrameworkCore;

namespace L2.Server.Context;

public sealed class L2ServerDbContext(DbContextOptions<L2ServerDbContext> options) : DbContext(options)
{
    public const string SchemaName = "public";

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<GameVersion> GameVersions => Set<GameVersion>();
    public DbSet<AccountCredential> AccountCredentials => Set<AccountCredential>();
    public DbSet<AccountSession> AccountSessions => Set<AccountSession>();
    public DbSet<AccountLoginHistory> AccountLoginHistory => Set<AccountLoginHistory>();
    public DbSet<GameSessionTicket> GameSessionTickets => Set<GameSessionTicket>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<PlayerCharacter> Characters => Set<PlayerCharacter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        ConfigureGameVersions(modelBuilder);
        ConfigurePlayerIdentity(modelBuilder);
        ConfigurePlayerCharacters(modelBuilder);
    }

    private static void ConfigureGameVersions(ModelBuilder modelBuilder)
    {
        var version = modelBuilder.Entity<GameVersion>();
        version.HasIndex(entity => entity.DisplayName).IsUnique()
            .HasDatabaseName("ix_game_versions_display_name");
        version.HasData(
            new GameVersion { Key = "c1", DisplayName = "Chronicle 1", SortOrder = 10 },
            new GameVersion { Key = "c4", DisplayName = "Chronicle 4", SortOrder = 20 },
            new GameVersion { Key = "interlude", DisplayName = "Interlude", SortOrder = 30 });
    }

    private static void ConfigurePlayerIdentity(ModelBuilder modelBuilder)
    {
        var account = modelBuilder.Entity<Account>();
        account.HasIndex(entity => entity.NormalizedUsername).IsUnique().HasDatabaseName("ix_accounts_normalized_username");
        account.HasIndex(entity => entity.NormalizedEmail).IsUnique().HasDatabaseName("ix_accounts_normalized_email");

        var credential = modelBuilder.Entity<AccountCredential>();
        credential.HasOne(entity => entity.Account)
            .WithOne(entity => entity.Credential)
            .HasForeignKey<AccountCredential>(entity => entity.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        var session = modelBuilder.Entity<AccountSession>();
        session.HasIndex(entity => entity.TokenHash).IsUnique().HasDatabaseName("ix_account_sessions_token_hash");
        session.HasIndex(entity => new { entity.AccountId, entity.ExpiresAt })
            .HasFilter("revoked_at IS NULL")
            .HasDatabaseName("ix_account_sessions_account_active");
        session.HasOne(entity => entity.Account)
            .WithMany(entity => entity.Sessions)
            .HasForeignKey(entity => entity.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
        session.HasOne(entity => entity.Version).WithMany()
            .HasForeignKey(entity => entity.GameVersion).OnDelete(DeleteBehavior.Restrict);

        var history = modelBuilder.Entity<AccountLoginHistory>();
        history.HasIndex(entity => new { entity.AccountId, entity.OccurredAt })
            .IsDescending(false, true)
            .HasDatabaseName("ix_account_login_history_account_time");
        history.HasOne(entity => entity.Account)
            .WithMany(entity => entity.LoginHistory)
            .HasForeignKey(entity => entity.AccountId)
            .OnDelete(DeleteBehavior.SetNull);
        history.HasOne(entity => entity.Version).WithMany()
            .HasForeignKey(entity => entity.GameVersion).OnDelete(DeleteBehavior.Restrict);

        var ticket = modelBuilder.Entity<GameSessionTicket>();
        ticket.HasIndex(entity => entity.TokenHash).IsUnique().HasDatabaseName("ix_game_session_tickets_token_hash");
        ticket.HasIndex(entity => entity.AccountSessionId)
            .HasDatabaseName("ix_game_session_tickets_account_session_id");
        ticket.HasIndex(entity => entity.ExpiresAt)
            .HasFilter("consumed_at IS NULL")
            .HasDatabaseName("ix_game_session_tickets_pending_expiry");
        ticket.HasOne(entity => entity.AccountSession)
            .WithMany(entity => entity.GameTickets)
            .HasForeignKey(entity => entity.AccountSessionId)
            .OnDelete(DeleteBehavior.Cascade);
        ticket.HasOne(entity => entity.Version).WithMany()
            .HasForeignKey(entity => entity.GameVersion).OnDelete(DeleteBehavior.Restrict);

        var gameSession = modelBuilder.Entity<GameSession>();
        gameSession.HasOne(entity => entity.AccountSession)
            .WithMany(entity => entity.GameSessions)
            .HasForeignKey(entity => entity.AccountSessionId)
            .OnDelete(DeleteBehavior.Cascade);
        gameSession.HasOne(entity => entity.Version).WithMany()
            .HasForeignKey(entity => entity.GameVersion).OnDelete(DeleteBehavior.Restrict);
        gameSession.HasIndex(entity => entity.AccountSessionId)
            .HasDatabaseName("ix_game_sessions_account_session_id");
        gameSession.HasIndex(entity => entity.AccessTokenHash).IsUnique()
            .HasDatabaseName("ix_game_sessions_access_token_hash");
        gameSession.HasIndex(entity => new { entity.RevokedAt, entity.ExpiresAt })
            .HasDatabaseName("ix_game_sessions_active_expiry");
    }

    private static void ConfigurePlayerCharacters(ModelBuilder modelBuilder)
    {
        var character = modelBuilder.Entity<PlayerCharacter>();
        character.ToTable("characters", table =>
        {
            table.HasCheckConstraint("ck_characters_level", "level BETWEEN 1 AND 255");
            table.HasCheckConstraint("ck_characters_experience", "experience >= 0");
            table.HasCheckConstraint("ck_characters_account_slot", "account_slot >= 0");
        });
        character.HasOne(entity => entity.Version).WithMany()
            .HasForeignKey(entity => entity.GameVersion).OnDelete(DeleteBehavior.Restrict);
        character.HasIndex(entity => new { entity.GameVersion, entity.NormalizedName }).IsUnique()
            .HasDatabaseName("ix_characters_normalized_name");
        character.HasIndex(entity => new { entity.GameVersion, entity.AccountId, entity.AccountSlot }).IsUnique()
            .HasDatabaseName("ix_characters_account_slot");
        character.HasIndex(entity => new { entity.GameVersion, entity.AccountId, entity.CreatedAt })
            .HasDatabaseName("ix_characters_account_created");
        character.HasIndex(entity => entity.DeleteAfter)
            .HasFilter("delete_after IS NOT NULL")
            .HasDatabaseName("ix_characters_deletion_deadline");
    }
}
