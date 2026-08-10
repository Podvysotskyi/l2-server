using L2.Server.Context.Identifiers;

namespace L2.Server.Context.Seeding;

public sealed record SkillSeedDefinition(
    int Id,
    short Levels,
    string Name,
    SkillOperateTypeId? SkillOperateTypeId,
    SkillTargetTypeId? SkillTargetTypeId);
