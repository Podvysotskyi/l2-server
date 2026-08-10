using L2.Server.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace L2.Server.Migrations;

public sealed class L2ServerDbContextFactory : IDesignTimeDbContextFactory<L2ServerDbContext>
{
    public L2ServerDbContext CreateDbContext(string[] args)
    {
        const string connectionString = "Host=localhost;Port=5432;Database=l2-server;Username=l2;Password=secret";
        var options = new DbContextOptionsBuilder<L2ServerDbContext>()
            .UseNpgsql(connectionString, postgres =>
            {
                postgres.MigrationsAssembly(typeof(MigrationAssemblyMarker).Assembly.FullName);
                postgres.MigrationsHistoryTable("__EFMigrationsHistory", L2ServerDbContext.SchemaName);
            })
            .Options;
        return new L2ServerDbContext(options);
    }
}
