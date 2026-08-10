using L2.Server.Context.Entities;
using Microsoft.EntityFrameworkCore;

namespace L2.Server.Context;

public sealed partial class L2ServerDbContext
{
    private static void ConfigurePlayerIdentity(ModelBuilder modelBuilder)
    {
        var account = modelBuilder.Entity<Account>();
        account.ToTable("accounts");
        account.HasKey(entity => entity.Id);
        account.Property(entity => entity.Id).HasColumnName("id");
        account.Property(entity => entity.Username).HasColumnName("username").HasMaxLength(24);
        account.Property(entity => entity.NormalizedUsername).HasColumnName("normalized_username").HasMaxLength(24);
        account.Property(entity => entity.Email).HasColumnName("email").HasMaxLength(254);
        account.Property(entity => entity.NormalizedEmail).HasColumnName("normalized_email").HasMaxLength(254);
        account.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        account.HasIndex(entity => entity.NormalizedUsername).IsUnique().HasDatabaseName("ix_accounts_normalized_username");
        account.HasIndex(entity => entity.NormalizedEmail).IsUnique().HasDatabaseName("ix_accounts_normalized_email");

        var credential = modelBuilder.Entity<AccountCredential>();
        credential.ToTable("account_credentials");
        credential.HasKey(entity => entity.AccountId);
        credential.Property(entity => entity.AccountId).HasColumnName("account_id");
        credential.Property(entity => entity.PasswordHash).HasColumnName("password_hash");
        credential.Property(entity => entity.UpdatedAt).HasColumnName("updated_at");
        credential.HasOne(entity => entity.Account)
            .WithOne(entity => entity.Credential)
            .HasForeignKey<AccountCredential>(entity => entity.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        var session = modelBuilder.Entity<AccountSession>();
        session.ToTable("account_sessions");
        session.HasKey(entity => entity.Id);
        session.Property(entity => entity.Id).HasColumnName("id");
        session.Property(entity => entity.AccountId).HasColumnName("account_id");
        session.Property(entity => entity.TokenHash).HasColumnName("token_hash");
        session.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        session.Property(entity => entity.LastSeenAt).HasColumnName("last_seen_at");
        session.Property(entity => entity.ExpiresAt).HasColumnName("expires_at");
        session.Property(entity => entity.RevokedAt).HasColumnName("revoked_at");
        session.HasIndex(entity => entity.TokenHash).IsUnique().HasDatabaseName("ix_account_sessions_token_hash");
        session.HasIndex(entity => new { entity.AccountId, entity.ExpiresAt })
            .HasFilter("revoked_at IS NULL")
            .HasDatabaseName("ix_account_sessions_account_active");
        session.HasOne(entity => entity.Account)
            .WithMany(entity => entity.Sessions)
            .HasForeignKey(entity => entity.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        var history = modelBuilder.Entity<AccountLoginHistory>();
        history.ToTable("account_login_history");
        history.HasKey(entity => entity.Id);
        history.Property(entity => entity.Id).HasColumnName("id");
        history.Property(entity => entity.AccountId).HasColumnName("account_id");
        history.Property(entity => entity.NormalizedEmail).HasColumnName("normalized_email").HasMaxLength(254);
        history.Property(entity => entity.Succeeded).HasColumnName("succeeded");
        history.Property(entity => entity.FailureCode).HasColumnName("failure_code").HasMaxLength(40);
        history.Property(entity => entity.IpAddress).HasColumnName("ip_address").HasMaxLength(64);
        history.Property(entity => entity.UserAgent).HasColumnName("user_agent").HasMaxLength(512);
        history.Property(entity => entity.OccurredAt).HasColumnName("occurred_at");
        history.HasIndex(entity => new { entity.AccountId, entity.OccurredAt })
            .IsDescending(false, true)
            .HasDatabaseName("ix_account_login_history_account_time");
        history.HasOne(entity => entity.Account)
            .WithMany(entity => entity.LoginHistory)
            .HasForeignKey(entity => entity.AccountId)
            .OnDelete(DeleteBehavior.SetNull);

        var ticket = modelBuilder.Entity<GameSessionTicket>();
        ticket.ToTable("game_session_tickets");
        ticket.HasKey(entity => entity.Id);
        ticket.Property(entity => entity.Id).HasColumnName("id");
        ticket.Property(entity => entity.AccountSessionId).HasColumnName("account_session_id");
        ticket.Property(entity => entity.TokenHash).HasColumnName("token_hash");
        ticket.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        ticket.Property(entity => entity.ExpiresAt).HasColumnName("expires_at");
        ticket.Property(entity => entity.ConsumedAt).HasColumnName("consumed_at");
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

        var gameSession = modelBuilder.Entity<GameSession>();
        gameSession.ToTable("game_sessions");
        gameSession.HasKey(entity => entity.Id);
        gameSession.Property(entity => entity.Id).HasColumnName("id");
        gameSession.Property(entity => entity.AccountSessionId).HasColumnName("account_session_id");
        gameSession.Property(entity => entity.AccessTokenHash).HasColumnName("access_token_hash");
        gameSession.Property(entity => entity.SelectedCharacterId).HasColumnName("selected_character_id");
        gameSession.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        gameSession.Property(entity => entity.LastSeenAt).HasColumnName("last_seen_at");
        gameSession.Property(entity => entity.ExpiresAt).HasColumnName("expires_at");
        gameSession.Property(entity => entity.RevokedAt).HasColumnName("revoked_at");
        gameSession.HasOne(entity => entity.AccountSession)
            .WithMany(entity => entity.GameSessions)
            .HasForeignKey(entity => entity.AccountSessionId)
            .OnDelete(DeleteBehavior.Cascade);
        gameSession.HasIndex(entity => entity.AccountSessionId)
            .HasDatabaseName("ix_game_sessions_account_session_id");
        gameSession.HasIndex(entity => entity.AccessTokenHash).IsUnique()
            .HasDatabaseName("ix_game_sessions_access_token_hash");
        gameSession.HasIndex(entity => new { entity.RevokedAt, entity.ExpiresAt })
            .HasDatabaseName("ix_game_sessions_active_expiry");
    }
}
