using L2.LoginServer.Authentication;
using L2.PlayerIdentity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace L2.LoginServer.Data;

public sealed class PlayerIdentityMigrator(
    IDbContextFactory<PlayerIdentityDbContext> contextFactory,
    IOptions<AuthenticationOptions> options,
    ILogger<PlayerIdentityMigrator> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.RunPlayerIdentityMigrations)
        {
            return;
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
        if (pending.Length == 0)
        {
            return;
        }

        await context.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Applied player identity migrations {Migrations}", pending);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
