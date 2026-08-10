using L2.GameContent.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace L2.GameContent.Seeding;

public sealed class PlayerLookupSeeder(
    IDbContextFactory<GameContentDbContext> contextFactory,
    ILogger<PlayerLookupSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var races = await context.PlayerRaces.ToDictionaryAsync(entity => entity.Id, cancellationToken);
        var sexes = await context.PlayerSexes.ToDictionaryAsync(entity => entity.Id, cancellationToken);

        foreach (var value in PlayerLookupSeedValues.Races)
        {
            if (races.TryGetValue(value.Id, out var race))
            {
                race.Name = value.Name;
            }
            else
            {
                context.PlayerRaces.Add(new PlayerRace { Id = value.Id, Name = value.Name });
            }
        }

        foreach (var value in PlayerLookupSeedValues.Sexes)
        {
            if (sexes.TryGetValue(value.Id, out var sex))
            {
                sex.Name = value.Name;
            }
            else
            {
                context.PlayerSexes.Add(new PlayerSex { Id = value.Id, Name = value.Name });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Seeded {PlayerRaceCount} player races and {PlayerSexCount} player sexes",
            PlayerLookupSeedValues.Races.Count,
            PlayerLookupSeedValues.Sexes.Count);
    }
}
