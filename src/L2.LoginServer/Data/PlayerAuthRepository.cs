using L2.LoginServer.Authentication;
using L2.PlayerIdentity;
using L2.PlayerIdentity.Entities;
using Microsoft.EntityFrameworkCore;

namespace L2.LoginServer.Data;

public sealed class PlayerAuthRepository(IDbContextFactory<PlayerIdentityDbContext> contextFactory)
{
    public async Task<bool> CreateAccountAsync(
        Guid accountId,
        string username,
        string normalizedUsername,
        string email,
        string normalizedEmail,
        string passwordHash,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var account = new Account
        {
            Id = accountId,
            Username = username,
            NormalizedUsername = normalizedUsername,
            Email = email,
            NormalizedEmail = normalizedEmail,
            CreatedAt = now,
            Credential = new L2.PlayerIdentity.Entities.AccountCredential
            {
                AccountId = accountId,
                PasswordHash = passwordHash,
                UpdatedAt = now
            }
        };
        context.Accounts.Add(account);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception exception) when (PlayerIdentityDatabase.IsUniqueViolation(exception))
        {
            return false;
        }
        catch (Exception exception) when (PlayerIdentityDatabase.IsPersistenceFailure(exception))
        {
            throw PlayerIdentityDatabase.Wrap("Player account creation failed.", exception);
        }
    }

    public async Task<Authentication.AccountCredential?> FindCredentialAsync(
        string normalizedEmail,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.AccountCredentials
                .AsNoTracking()
                .Where(credential => credential.Account.NormalizedEmail == normalizedEmail)
                .Select(credential => new Authentication.AccountCredential(
                    credential.AccountId,
                    credential.Account.Username,
                    credential.PasswordHash))
                .SingleOrDefaultAsync(cancellationToken);
        }
        catch (Exception exception) when (PlayerIdentityDatabase.IsPersistenceFailure(exception))
        {
            throw PlayerIdentityDatabase.Wrap("Credential lookup failed.", exception);
        }
    }

    public async Task CreateLoginSessionAsync(
        Authentication.AccountCredential credential,
        string normalizedEmail,
        string? replacementPasswordHash,
        byte[] tokenHash,
        DateTimeOffset now,
        DateTimeOffset expiresAt,
        RequestMetadata metadata,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            await context.AccountSessions
                .Where(session => session.AccountId == credential.AccountId && session.RevokedAt == null)
                .ExecuteUpdateAsync(update => update.SetProperty(session => session.RevokedAt, now), cancellationToken);
            if (replacementPasswordHash is not null)
            {
                await context.AccountCredentials
                    .Where(existing => existing.AccountId == credential.AccountId)
                    .ExecuteUpdateAsync(update => update
                        .SetProperty(existing => existing.PasswordHash, replacementPasswordHash)
                        .SetProperty(existing => existing.UpdatedAt, now), cancellationToken);
            }

            context.AccountSessions.Add(CreateSession(credential.AccountId, tokenHash, now, expiresAt));
            context.AccountLoginHistory.Add(CreateHistory(
                credential.AccountId, normalizedEmail, true, null, metadata, now));
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception) when (PlayerIdentityDatabase.IsPersistenceFailure(exception))
        {
            throw PlayerIdentityDatabase.Wrap("Login session creation failed.", exception);
        }
    }

    public async Task RecordFailedLoginAsync(
        Guid? accountId,
        string normalizedEmail,
        RequestMetadata metadata,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            context.AccountLoginHistory.Add(CreateHistory(
                accountId, normalizedEmail, false, "invalid_credentials", metadata, now));
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (PlayerIdentityDatabase.IsPersistenceFailure(exception))
        {
            throw PlayerIdentityDatabase.Wrap("Failed login recording failed.", exception);
        }
    }

    public async Task<SessionLookup?> FindSessionAsync(
        byte[] tokenHash,
        DateTimeOffset now,
        DateTimeOffset refreshedExpiry,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var session = await context.AccountSessions
                .Include(existing => existing.Account)
                .SingleOrDefaultAsync(existing =>
                    existing.TokenHash.SequenceEqual(tokenHash) &&
                    existing.RevokedAt == null &&
                    existing.ExpiresAt > now,
                    cancellationToken);
            if (session is null)
            {
                return null;
            }

            var refreshed = session.LastSeenAt <= now.AddMinutes(-5);
            if (refreshed)
            {
                session.LastSeenAt = now;
                session.ExpiresAt = refreshedExpiry;
                await context.SaveChangesAsync(cancellationToken);
            }

            return new SessionLookup(
                new AuthSession(session.AccountId, session.Account.Username, session.ExpiresAt),
                refreshed);
        }
        catch (Exception exception) when (PlayerIdentityDatabase.IsPersistenceFailure(exception))
        {
            throw PlayerIdentityDatabase.Wrap("Login session lookup failed.", exception);
        }
    }

    public async Task<bool> CreateGameTicketAsync(
        byte[] sessionTokenHash,
        byte[] ticketTokenHash,
        DateTimeOffset now,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            await context.GameSessionTickets
                .Where(ticket => ticket.ExpiresAt < now.AddHours(-24))
                .ExecuteDeleteAsync(cancellationToken);
            var sessionId = await context.AccountSessions
                .Where(session => session.TokenHash.SequenceEqual(sessionTokenHash) &&
                    session.RevokedAt == null && session.ExpiresAt > now)
                .Select(session => (Guid?)session.Id)
                .SingleOrDefaultAsync(cancellationToken);
            if (sessionId is null)
            {
                await transaction.CommitAsync(cancellationToken);
                return false;
            }

            context.GameSessionTickets.Add(new GameSessionTicket
            {
                Id = Guid.NewGuid(),
                AccountSessionId = sessionId.Value,
                TokenHash = ticketTokenHash,
                CreatedAt = now,
                ExpiresAt = expiresAt
            });
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception exception) when (PlayerIdentityDatabase.IsPersistenceFailure(exception))
        {
            throw PlayerIdentityDatabase.Wrap("Game session ticket creation failed.", exception);
        }
    }

    public async Task RevokeSessionAsync(byte[] tokenHash, DateTimeOffset now, CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.AccountSessions
                .Where(session => session.TokenHash.SequenceEqual(tokenHash) && session.RevokedAt == null)
                .ExecuteUpdateAsync(update => update.SetProperty(session => session.RevokedAt, now), cancellationToken);
        }
        catch (Exception exception) when (PlayerIdentityDatabase.IsPersistenceFailure(exception))
        {
            throw PlayerIdentityDatabase.Wrap("Login session revocation failed.", exception);
        }
    }

    private static AccountSession CreateSession(
        Guid accountId,
        byte[] tokenHash,
        DateTimeOffset now,
        DateTimeOffset expiresAt) => new()
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            TokenHash = tokenHash,
            CreatedAt = now,
            LastSeenAt = now,
            ExpiresAt = expiresAt
        };

    private static AccountLoginHistory CreateHistory(
        Guid? accountId,
        string normalizedEmail,
        bool succeeded,
        string? failureCode,
        RequestMetadata metadata,
        DateTimeOffset now) => new()
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            NormalizedEmail = normalizedEmail,
            Succeeded = succeeded,
            FailureCode = failureCode,
            IpAddress = metadata.IpAddress,
            UserAgent = metadata.UserAgent?[..Math.Min(metadata.UserAgent.Length, 512)],
            OccurredAt = now
        };
}
