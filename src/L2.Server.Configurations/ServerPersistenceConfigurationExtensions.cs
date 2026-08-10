using L2.Server.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace L2.Server.Configurations;

public static class ServerPersistenceConfigurationExtensions
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
        services.AddPooledDbContextFactory<L2ServerDbContext>(options => options.UseNpgsql(
            connectionString,
            postgres =>
            {
                postgres.MigrationsAssembly(typeof(L2.Server.Migrations.MigrationAssemblyMarker).Assembly.FullName);
                postgres.MigrationsHistoryTable("__EFMigrationsHistory", L2ServerDbContext.SchemaName);
            }));
        services.AddHostedService<L2ServerMigrator>();
        services.AddHealthChecks().AddCheck<ServerMigrationHealthCheck>(
            "server-migrations",
            tags: ["ready"]);
        return services;
    }
}
