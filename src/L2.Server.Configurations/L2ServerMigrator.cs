using L2.Server.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace L2.Server.Configurations;

public sealed class L2ServerMigrator(
    IDbContextFactory<L2ServerDbContext> contextFactory,
    IOptions<ServerPersistenceOptions> options,
    ILogger<L2ServerMigrator> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.RunMigrations)
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
        logger.LogInformation("Applied server migrations {Migrations}", pending);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
