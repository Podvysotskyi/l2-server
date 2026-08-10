using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace L2.GameContent;

public sealed class GameContentDbContextFactory : IDesignTimeDbContextFactory<GameContentDbContext>
{
    public GameContentDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql")
            ?? "Host=localhost;Port=5432;Database=l2web;Username=l2web;Password=l2web_dev";
        var options = new DbContextOptionsBuilder<GameContentDbContext>()
            .UseNpgsql(connectionString, postgres => postgres.MigrationsHistoryTable(
                "__EFMigrationsHistory",
                GameContentDbContext.SchemaName))
            .Options;
        return new GameContentDbContext(options);
    }
}
