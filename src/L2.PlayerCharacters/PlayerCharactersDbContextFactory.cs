using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace L2.PlayerCharacters;

public sealed class PlayerCharactersDbContextFactory : IDesignTimeDbContextFactory<PlayerCharactersDbContext>
{
    public PlayerCharactersDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql")
            ?? "Host=localhost;Port=5432;Database=l2web;Username=l2web;Password=l2web_dev";
        var options = new DbContextOptionsBuilder<PlayerCharactersDbContext>()
            .UseNpgsql(connectionString, postgres => postgres.MigrationsHistoryTable(
                "__EFMigrationsHistory",
                PlayerCharactersDbContext.SchemaName))
            .Options;
        return new PlayerCharactersDbContext(options);
    }
}
