using L2.Server.Context;
using L2.Server.Context.Seeding;
using L2.Server.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace L2.Server.Configurations;

public static class ServerPersistenceExtensions
{
    public static IServiceCollection AddServerPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("ConnectionStrings:PostgreSql is required.");
        services.AddOptions<ServerPersistenceOptions>()
            .Bind(configuration.GetSection(ServerPersistenceOptions.SectionName))
            .ValidateOnStart();
        services.AddOptions<PlayerCharacterOptions>()
            .Bind(configuration.GetSection(PlayerCharacterOptions.SectionName))
            .Validate(value => value.MaximumCharactersPerAccount > 0, "Character limit must be positive.")
            .Validate(value => value.MinimumNameLength > 0 &&
                value.MaximumNameLength >= value.MinimumNameLength && value.MaximumNameLength <= 16,
                "Character name limits must be between 1 and 16.")
            .ValidateOnStart();
        services.AddPooledDbContextFactory<L2ServerDbContext>(options => options.UseNpgsql(
            connectionString,
            postgres =>
            {
                postgres.MigrationsAssembly(typeof(L2.Server.Migrations.MigrationAssemblyMarker).Assembly.FullName);
                postgres.MigrationsHistoryTable("__EFMigrationsHistory", L2ServerDbContext.SchemaName);
            }));
        services.AddSingleton<NpcLookupSeeder>();
        services.AddSingleton<PlayerLookupSeeder>();
        services.AddSingleton<PlayerClassSeeder>();
        services.AddSingleton<PlayerAppearanceSeeder>();
        services.AddSingleton<NpcSeeder>();
        services.AddSingleton<SkillSeeder>();
        services.AddHostedService<L2ServerMigrator>();
        services.AddHealthChecks().AddCheck<ServerMigrationHealthCheck>("server-migrations", tags: ["ready"]);
        return services;
    }
}

public sealed class ServerMigrationHealthCheck(
    IDbContextFactory<L2ServerDbContext> contextFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
            var pending = (await database.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
            return pending.Length == 0
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"Pending server migrations: {string.Join(", ", pending)}");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Server migration state could not be checked.", exception);
        }
    }
}
