using L2.GameContent.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace L2.GameContent.Seeding;

public sealed class SkillSeeder(
    IDbContextFactory<GameContentDbContext> contextFactory,
    ILogger<SkillSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var existingOperateTypes = await context.SkillOperateTypes
            .ToDictionaryAsync(entity => entity.Id, cancellationToken);
        var addedOperateTypes = 0;
        var updatedOperateTypes = 0;
        foreach (var definition in SkillSeedValues.OperateTypes)
        {
            if (existingOperateTypes.TryGetValue(definition.Id, out var operateType))
            {
                operateType.Name = definition.Name;
                updatedOperateTypes++;
            }
            else
            {
                context.SkillOperateTypes.Add(new SkillOperateType { Id = definition.Id, Name = definition.Name });
                addedOperateTypes++;
            }
        }

        var existingTargetTypes = await context.SkillTargetTypes
            .ToDictionaryAsync(entity => entity.Id, cancellationToken);
        var addedTargetTypes = 0;
        var updatedTargetTypes = 0;
        foreach (var definition in SkillSeedValues.TargetTypes)
        {
            if (existingTargetTypes.TryGetValue(definition.Id, out var targetType))
            {
                targetType.Name = definition.Name;
                updatedTargetTypes++;
            }
            else
            {
                context.SkillTargetTypes.Add(new SkillTargetType { Id = definition.Id, Name = definition.Name });
                addedTargetTypes++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        var existingSkills = await context.Skills.ToDictionaryAsync(entity => entity.Id, cancellationToken);
        var addedSkills = 0;
        var updatedSkills = 0;

        foreach (var definition in SkillSeedValues.Skills)
        {
            if (existingSkills.TryGetValue(definition.Id, out var skill))
            {
                skill.Levels = definition.Levels;
                skill.Name = definition.Name;
                skill.SkillOperateTypeId = definition.SkillOperateTypeId;
                skill.SkillTargetTypeId = definition.SkillTargetTypeId;
                updatedSkills++;
            }
            else
            {
                context.Skills.Add(new Skill
                {
                    Id = definition.Id,
                    Levels = definition.Levels,
                    Name = definition.Name,
                    SkillOperateTypeId = definition.SkillOperateTypeId,
                    SkillTargetTypeId = definition.SkillTargetTypeId
                });
                addedSkills++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        var existingIcons = (await context.SkillIcons.ToListAsync(cancellationToken))
            .ToDictionary(entity => (entity.SkillId, entity.Level));
        var addedIcons = 0;
        var updatedIcons = 0;
        foreach (var definition in SkillSeedValues.Icons)
        {
            if (existingIcons.TryGetValue((definition.SkillId, definition.Level), out var icon))
            {
                icon.Name = definition.Name;
                updatedIcons++;
            }
            else
            {
                context.SkillIcons.Add(new SkillIcon
                {
                    SkillId = definition.SkillId,
                    Level = definition.Level,
                    Name = definition.Name
                });
                addedIcons++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Seeded {IconCount} skill icons ({AddedIconCount} added, {UpdatedIconCount} updated), " +
            "{OperateTypeCount} operate types ({AddedOperateTypeCount} added, {UpdatedOperateTypeCount} updated), " +
            "{TargetTypeCount} target types ({AddedTargetTypeCount} added, {UpdatedTargetTypeCount} updated), and " +
            "{SkillCount} skills ({AddedSkillCount} added, {UpdatedSkillCount} updated)",
            SkillSeedValues.Icons.Count,
            addedIcons,
            updatedIcons,
            SkillSeedValues.OperateTypes.Count,
            addedOperateTypes,
            updatedOperateTypes,
            SkillSeedValues.TargetTypes.Count,
            addedTargetTypes,
            updatedTargetTypes,
            SkillSeedValues.Skills.Count,
            addedSkills,
            updatedSkills);
    }
}
