using L2.GameContent.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace L2.GameContent.Seeding;

public sealed class PlayerAppearanceSeeder(
    IDbContextFactory<GameContentDbContext> contextFactory,
    ILogger<PlayerAppearanceSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var faces = await context.PlayerFaces.ToDictionaryAsync(
            item => (item.Id, item.PlayerSexId, item.PlayerRaceId), cancellationToken);
        var styles = await context.PlayerHairStyles.ToDictionaryAsync(
            item => (item.Id, item.PlayerSexId, item.PlayerRaceId), cancellationToken);
        var colors = await context.PlayerHairColors.ToDictionaryAsync(
            item => (item.Id, item.PlayerSexId, item.PlayerRaceId), cancellationToken);
        foreach (var value in PlayerAppearanceSeedValues.Faces)
        {
            if (faces.TryGetValue((value.Id, value.PlayerSexId, value.PlayerRaceId), out var item))
                item.Name = value.Name;
            else context.PlayerFaces.Add(new PlayerFace
            {
                Id = value.Id, Name = value.Name,
                PlayerSexId = value.PlayerSexId, PlayerRaceId = value.PlayerRaceId
            });
        }
        foreach (var value in PlayerAppearanceSeedValues.HairStyles)
        {
            if (styles.TryGetValue((value.Id, value.PlayerSexId, value.PlayerRaceId), out var item))
                item.Name = value.Name;
            else context.PlayerHairStyles.Add(new PlayerHairStyle
            {
                Id = value.Id, Name = value.Name,
                PlayerSexId = value.PlayerSexId, PlayerRaceId = value.PlayerRaceId
            });
        }
        foreach (var value in PlayerAppearanceSeedValues.HairColors)
        {
            if (colors.TryGetValue((value.Id, value.PlayerSexId, value.PlayerRaceId), out var item))
                item.Name = value.Name;
            else context.PlayerHairColors.Add(new PlayerHairColor
            {
                Id = value.Id, Name = value.Name,
                PlayerSexId = value.PlayerSexId, PlayerRaceId = value.PlayerRaceId
            });
        }
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Seeded {FaceCount} faces, {HairStyleCount} hair styles, and {HairColorCount} hair colors",
            PlayerAppearanceSeedValues.Faces.Count,
            PlayerAppearanceSeedValues.HairStyles.Count,
            PlayerAppearanceSeedValues.HairColors.Count);
    }
}
