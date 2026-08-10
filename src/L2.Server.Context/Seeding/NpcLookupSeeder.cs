using L2.Server.Context;
using L2.Server.Context.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace L2.Server.Context.Seeding;

public sealed class NpcLookupSeeder(
    IDbContextFactory<L2ServerDbContext> contextFactory,
    ILogger<NpcLookupSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var types = await context.NpcTypes.ToDictionaryAsync(entity => entity.Id, cancellationToken);
        foreach (var value in NpcLookupSeedValues.Types)
        {
            if (types.TryGetValue(value.Id, out var existing))
            {
                existing.Name = value.Name;
            }
            else
            {
                context.NpcTypes.Add(new NpcType { Id = value.Id, Name = value.Name });
            }
        }

        var races = await context.NpcRaces.ToDictionaryAsync(entity => entity.Id, cancellationToken);
        foreach (var value in NpcLookupSeedValues.Races)
        {
            if (races.TryGetValue(value.Id, out var existing))
            {
                existing.Name = value.Name;
            }
            else
            {
                context.NpcRaces.Add(new NpcRace { Id = value.Id, Name = value.Name });
            }
        }

        var sexes = await context.NpcSexes.ToDictionaryAsync(entity => entity.Id, cancellationToken);
        foreach (var value in NpcLookupSeedValues.Sexes)
        {
            if (sexes.TryGetValue(value.Id, out var existing))
            {
                existing.Name = value.Name;
            }
            else
            {
                context.NpcSexes.Add(new NpcSex { Id = value.Id, Name = value.Name });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Seeded {NpcTypeCount} NPC types, {NpcRaceCount} NPC races, and {NpcSexCount} NPC sexes",
            NpcLookupSeedValues.Types.Count,
            NpcLookupSeedValues.Races.Count,
            NpcLookupSeedValues.Sexes.Count);
    }
}
