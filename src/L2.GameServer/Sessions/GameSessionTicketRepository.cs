using L2.PlayerIdentity;
using L2.Shared;
using Microsoft.EntityFrameworkCore;

namespace L2.GameServer.Sessions;

public sealed class GameSessionTicketRepository(
    IDbContextFactory<PlayerIdentityDbContext> contextFactory,
    TimeProvider timeProvider)
{
    public async Task<AuthenticatedAccount?> ConsumeAsync(string ticket, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var tokenHash = GameSessionTicketToken.Hash(ticket);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        try
        {
            var consumed = await context.GameSessionTickets
                .Where(candidate =>
                    candidate.TokenHash.SequenceEqual(tokenHash) &&
                    candidate.ConsumedAt == null &&
                    candidate.ExpiresAt > now &&
                    candidate.AccountSession.RevokedAt == null &&
                    candidate.AccountSession.ExpiresAt > now)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(candidate => candidate.ConsumedAt, now),
                    cancellationToken);

            if (consumed != 1)
            {
                return null;
            }

            return await context.GameSessionTickets
                .AsNoTracking()
                .Where(candidate =>
                    candidate.TokenHash.SequenceEqual(tokenHash) &&
                    candidate.ConsumedAt == now)
                .Select(candidate => new AuthenticatedAccount(
                    candidate.AccountSession.Account.Id,
                    candidate.AccountSession.Account.Username))
                .SingleAsync(cancellationToken);
        }
        catch (Exception exception) when (PlayerIdentityDatabase.IsPersistenceFailure(exception))
        {
            throw PlayerIdentityDatabase.Wrap("Game session ticket redemption failed.", exception);
        }
    }
}
