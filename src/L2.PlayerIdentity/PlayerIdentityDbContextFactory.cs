using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace L2.PlayerIdentity;

public sealed class PlayerIdentityDbContextFactory : IDesignTimeDbContextFactory<PlayerIdentityDbContext>
{
    public PlayerIdentityDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql")
            ?? "Host=localhost;Port=5432;Database=l2web;Username=l2web;Password=l2web_dev";
        var options = new DbContextOptionsBuilder<PlayerIdentityDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new PlayerIdentityDbContext(options);
    }
}
