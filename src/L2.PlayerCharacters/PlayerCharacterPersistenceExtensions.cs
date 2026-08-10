using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace L2.PlayerCharacters;

public static class PlayerCharacterPersistenceExtensions
{
    public static IServiceCollection AddPlayerCharacterReadPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("ConnectionStrings:PostgreSql is required.");
        services.AddPooledDbContextFactory<PlayerCharactersDbContext>(db => ConfigureDatabase(db, connectionString));
        return services;
    }

    public static IServiceCollection AddPlayerCharacterPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("ConnectionStrings:PostgreSql is required.");
        services.AddOptions<PlayerCharacterOptions>()
            .Bind(configuration.GetSection(PlayerCharacterOptions.SectionName))
            .Validate(value => value.MaximumCharactersPerAccount > 0, "Character limit must be positive.")
            .Validate(value => value.MinimumNameLength > 0 &&
                value.MaximumNameLength >= value.MinimumNameLength && value.MaximumNameLength <= 16,
                "Character name limits must be between 1 and 16.")
            .ValidateOnStart();
        services.AddPooledDbContextFactory<PlayerCharactersDbContext>(db => ConfigureDatabase(db, connectionString));
        services.AddSingleton<PlayerCharacterService>();
        services.AddHostedService<PlayerCharacterMigrator>();
        return services;
    }

    private static void ConfigureDatabase(DbContextOptionsBuilder db, string connectionString) => db.UseNpgsql(
        connectionString,
        postgres =>
        {
            postgres.MigrationsAssembly(typeof(PlayerCharactersDbContext).Assembly.FullName);
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", PlayerCharactersDbContext.SchemaName);
        });

    public static IHealthChecksBuilder AddPlayerCharacterMigrationHealthCheck(this IHealthChecksBuilder checks) =>
        checks.AddCheck<PlayerCharacterMigrationHealthCheck>("player-character-migrations", tags: ["ready"]);
}

public sealed class PlayerCharacterMigrator(
    IDbContextFactory<PlayerCharactersDbContext> contextFactory,
    Microsoft.Extensions.Options.IOptions<PlayerCharacterOptions> options) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.RunMigrations) return;
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public sealed class PlayerCharacterMigrationHealthCheck(
    IDbContextFactory<PlayerCharactersDbContext> contextFactory) : IHealthCheck
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
                : HealthCheckResult.Unhealthy($"Pending player character migrations: {string.Join(", ", pending)}");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Player character migration state could not be checked.", exception);
        }
    }
}
