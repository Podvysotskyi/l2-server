using L2.Server.Context;
using L2.Server.Context.Entities;
using L2.Server.Repositories.Interfaces;
using L2.Server.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace L2.Server.Repositories;

public sealed class GameSessionRepository(IDbContextFactory<L2ServerDbContext> contextFactory)
    : IGameSessionRepository
{
    public async Task<GameSessionRecord?> RedeemAsync(
        byte[] ticketTokenHash,
        byte[] accessTokenHash,
        string gameVersion,
        string gameServer,
        Guid sessionId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var pendingTicket = await context.GameSessionTickets
                .Include(candidate => candidate.AccountSession)
                .ThenInclude(accountSession => accountSession.Account)
                .SingleOrDefaultAsync(candidate =>
                    candidate.TokenHash.SequenceEqual(ticketTokenHash) && candidate.ConsumedAt == null &&
                    candidate.GameVersion == gameVersion && candidate.GameServer == gameServer &&
                    candidate.ExpiresAt > now && candidate.AccountSession.RevokedAt == null &&
                    candidate.AccountSession.ExpiresAt > now, cancellationToken);
            if (pendingTicket is null)
            {
                await transaction.CommitAsync(cancellationToken);
                return null;
            }

            pendingTicket.ConsumedAt = now;
            var gameSession = new GameSession
            {
                Id = sessionId,
                AccountSessionId = pendingTicket.AccountSessionId,
                GameVersion = pendingTicket.GameVersion,
                GameServer = pendingTicket.GameServer,
                AccessTokenHash = accessTokenHash,
                CreatedAt = now,
                LastSeenAt = now,
                ExpiresAt = pendingTicket.AccountSession.ExpiresAt
            };
            context.GameSessions.Add(gameSession);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new GameSessionRecord(gameSession.Id, pendingTicket.AccountSession.AccountId,
                pendingTicket.AccountSession.Account.Username, gameSession.GameVersion, gameSession.GameServer,
                null, gameSession.ExpiresAt);
        }
        catch (Exception exception) when (PostgreSqlExceptionClassifier.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Game session exchange failed.", exception);
        }
    }

    public async Task<GameSessionRecord?> FindActiveAsync(
        byte[] accessTokenHash,
        string gameVersion,
        string gameServer,
        DateTimeOffset now,
        DateTimeOffset idleCutoff,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var session = await context.GameSessions
                .Include(candidate => candidate.AccountSession)
                .ThenInclude(accountSession => accountSession.Account)
                .SingleOrDefaultAsync(candidate => candidate.AccessTokenHash.SequenceEqual(accessTokenHash) &&
                    candidate.GameVersion == gameVersion && candidate.GameServer == gameServer &&
                    candidate.RevokedAt == null &&
                    candidate.ExpiresAt > now && candidate.LastSeenAt > idleCutoff &&
                    candidate.AccountSession.RevokedAt == null && candidate.AccountSession.ExpiresAt > now,
                    cancellationToken);
            if (session is null) return null;
            if (session.LastSeenAt <= now.AddMinutes(-1))
            {
                session.LastSeenAt = now;
                await context.SaveChangesAsync(cancellationToken);
            }
            return new GameSessionRecord(session.Id, session.AccountSession.AccountId,
                session.AccountSession.Account.Username, session.GameVersion, session.GameServer,
                session.SelectedCharacterId, session.ExpiresAt);
        }
        catch (Exception exception) when (PostgreSqlExceptionClassifier.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Game session lookup failed.", exception);
        }
    }

    public async Task SelectCharacterAsync(
        Guid sessionId,
        Guid characterId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.GameSessions.Where(session => session.Id == sessionId && session.RevokedAt == null)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(session => session.SelectedCharacterId, characterId)
                    .SetProperty(session => session.LastSeenAt, now), cancellationToken);
        }
        catch (Exception exception) when (PostgreSqlExceptionClassifier.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Game character selection failed.", exception);
        }
    }

    public async Task ClearCharacterAsync(Guid sessionId, Guid characterId, CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.GameSessions
                .Where(session => session.Id == sessionId && session.SelectedCharacterId == characterId)
                .ExecuteUpdateAsync(update => update.SetProperty(session => session.SelectedCharacterId, (Guid?)null),
                    cancellationToken);
        }
        catch (Exception exception) when (PostgreSqlExceptionClassifier.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Game character selection clearing failed.", exception);
        }
    }

    public async Task RevokeAsync(Guid sessionId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.GameSessions.Where(session => session.Id == sessionId && session.RevokedAt == null)
                .ExecuteUpdateAsync(update => update.SetProperty(session => session.RevokedAt, now),
                    cancellationToken);
        }
        catch (Exception exception) when (PostgreSqlExceptionClassifier.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Game session revocation failed.", exception);
        }
    }
}
