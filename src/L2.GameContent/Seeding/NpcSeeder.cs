using L2.GameContent.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace L2.GameContent.Seeding;

public sealed class NpcSeeder(
    IDbContextFactory<GameContentDbContext> contextFactory,
    ILogger<NpcSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var existingNpcs = await context.Npcs.ToDictionaryAsync(entity => entity.Id, cancellationToken);
        var added = 0;
        var updated = 0;

        foreach (var definition in NpcSeedValues.Npcs)
        {
            if (existingNpcs.TryGetValue(definition.Id, out var npc))
            {
                npc.Level = definition.Level;
                npc.Name = definition.Name;
                npc.NpcTypeId = definition.NpcTypeId;
                npc.NpcRaceId = definition.NpcRaceId;
                npc.NpcSexId = definition.NpcSexId;
                updated++;
            }
            else
            {
                context.Npcs.Add(new Npc
                {
                    Id = definition.Id,
                    Level = definition.Level,
                    Name = definition.Name,
                    NpcTypeId = definition.NpcTypeId,
                    NpcRaceId = definition.NpcRaceId,
                    NpcSexId = definition.NpcSexId
                });
                added++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Seeded {NpcCount} NPC definitions: {AddedCount} added and {UpdatedCount} updated",
            NpcSeedValues.Npcs.Count,
            added,
            updated);
    }
}
