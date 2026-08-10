using L2.GameContent.Identifiers;

namespace L2.GameContent.Seeding;

public sealed record SkillSeedDefinition(
    int Id,
    short Levels,
    string Name,
    SkillOperateTypeId? SkillOperateTypeId,
    SkillTargetTypeId? SkillTargetTypeId);
