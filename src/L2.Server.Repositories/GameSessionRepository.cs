using L2.Server.Context;
using L2.Server.Context.Entities;
using L2.Server.Repositories.Interfaces;
using L2.Server.Exceptions;
using L2.Server.Services.Interfaces;
using L2.Server.Contracts.Security;
using Microsoft.EntityFrameworkCore;

namespace L2.Server.Repositories;

public sealed class GameSessionRepository(
    IDbContextFactory<L2ServerDbContext> contextFactory,
    TimeProvider timeProvider) : IGameSessionRepository
{
    public async Task<GameSessionState?> RedeemAsync(
        string ticket,
        byte[] accessTokenHash,
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var ticketHash = GameSessionToken.Hash(ticket);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var pendingTicket = await context.GameSessionTickets
                .Include(candidate => candidate.AccountSession)
                .ThenInclude(accountSession => accountSession.Account)
                .SingleOrDefaultAsync(candidate =>
                    candidate.TokenHash.SequenceEqual(ticketHash) && candidate.ConsumedAt == null &&
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
                AccessTokenHash = accessTokenHash,
                CreatedAt = now,
                LastSeenAt = now,
                ExpiresAt = pendingTicket.AccountSession.ExpiresAt
            };
            context.GameSessions.Add(gameSession);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new GameSessionState(gameSession.Id, pendingTicket.AccountSession.AccountId,
                pendingTicket.AccountSession.Account.Username, null, gameSession.ExpiresAt);
        }
        catch (Exception exception) when (PlayerIdentityDatabase.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Game session exchange failed.", exception);
        }
    }

    public async Task<GameSessionState?> FindActiveAsync(
        byte[] accessTokenHash,
        DateTimeOffset idleCutoff,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var session = await context.GameSessions
                .Include(candidate => candidate.AccountSession)
                .ThenInclude(accountSession => accountSession.Account)
                .SingleOrDefaultAsync(candidate => candidate.AccessTokenHash.SequenceEqual(accessTokenHash) &&
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
            return new GameSessionState(session.Id, session.AccountSession.AccountId,
                session.AccountSession.Account.Username, session.SelectedCharacterId, session.ExpiresAt);
        }
        catch (Exception exception) when (PlayerIdentityDatabase.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Game session lookup failed.", exception);
        }
    }

    public async Task SelectCharacterAsync(Guid sessionId, Guid characterId, CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.GameSessions.Where(session => session.Id == sessionId && session.RevokedAt == null)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(session => session.SelectedCharacterId, characterId)
                    .SetProperty(session => session.LastSeenAt, timeProvider.GetUtcNow()), cancellationToken);
        }
        catch (Exception exception) when (PlayerIdentityDatabase.IsPersistenceFailure(exception))
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
        catch (Exception exception) when (PlayerIdentityDatabase.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Game character selection clearing failed.", exception);
        }
    }

    public async Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.GameSessions.Where(session => session.Id == sessionId && session.RevokedAt == null)
                .ExecuteUpdateAsync(update => update.SetProperty(session => session.RevokedAt, timeProvider.GetUtcNow()),
                    cancellationToken);
        }
        catch (Exception exception) when (PlayerIdentityDatabase.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Game session revocation failed.", exception);
        }
    }
}
