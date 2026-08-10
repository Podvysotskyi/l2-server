using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace L2.PlayerIdentity;

public static class PlayerIdentityPersistenceExtensions
{
    public static IServiceCollection AddPlayerIdentityPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("ConnectionStrings:PostgreSql is required.");
        services.AddPooledDbContextFactory<PlayerIdentityDbContext>(options => options.UseNpgsql(
            connectionString,
            postgres => postgres.MigrationsAssembly(typeof(PlayerIdentityDbContext).Assembly.FullName)));
        return services;
    }

    public static IHealthChecksBuilder AddPlayerIdentityMigrationHealthCheck(this IHealthChecksBuilder checks) =>
        checks.AddCheck<PlayerIdentityMigrationHealthCheck>("player-identity-migrations", tags: ["ready"]);
}

public sealed class PlayerIdentityMigrationHealthCheck(
    IDbContextFactory<PlayerIdentityDbContext> contextFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
            var pending = await database.Database.GetPendingMigrationsAsync(cancellationToken);
            var migrations = pending.ToArray();
            return migrations.Length == 0
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"Pending player identity migrations: {string.Join(", ", migrations)}");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Player identity migration state could not be checked.", exception);
        }
    }
}
