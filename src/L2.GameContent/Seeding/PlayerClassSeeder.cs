using L2.GameContent.Entities;
using L2.GameContent.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace L2.GameContent.Seeding;

public sealed class PlayerClassSeeder(
    IDbContextFactory<GameContentDbContext> contextFactory,
    ILogger<PlayerClassSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var existingRows = await context.PlayerClasses.ToListAsync(cancellationToken);
        var existingPlayerClasses = existingRows.ToDictionary(
            entity => (entity.Id, entity.PlayerSexId, entity.PlayerRaceId));
        var desiredKeys = new HashSet<(PlayerClassId, PlayerSexId, PlayerRaceId)>();
        var added = 0;
        var updated = 0;

        foreach (var definition in PlayerClassSeedValues.PlayerClasses)
        {
            foreach (var race in definition.AllowedRaces)
            {
                foreach (var sexId in race.AllowedSexIds)
                {
                    var key = (definition.Id, sexId, race.Id);
                    desiredKeys.Add(key);
                    if (existingPlayerClasses.TryGetValue(key, out var playerClass))
                    {
                        playerClass.Name = definition.Name;
                        playerClass.ParentClassId = definition.ParentClassId;
                        playerClass.IsMage = definition.IsMage;
                        updated++;
                    }
                    else
                    {
                        context.PlayerClasses.Add(new PlayerClass
                        {
                            Id = definition.Id,
                            PlayerSexId = sexId,
                            PlayerRaceId = race.Id,
                            Name = definition.Name,
                            ParentClassId = definition.ParentClassId,
                            IsMage = definition.IsMage
                        });
                        added++;
                    }
                }
            }
        }

        var removed = existingRows
            .Where(entity => Enum.IsDefined(entity.Id) &&
                !desiredKeys.Contains((entity.Id, entity.PlayerSexId, entity.PlayerRaceId)))
            .ToArray();
        context.PlayerClasses.RemoveRange(removed);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Seeded {PlayerClassCount} player class variants: " +
            "{AddedCount} added, {UpdatedCount} updated, and {RemovedCount} removed",
            desiredKeys.Count,
            added,
            updated,
            removed.Length);
    }
}
