using L2.GameContent.Identifiers;

namespace L2.GameContent.Entities;

public sealed class SkillTargetType
{
    public SkillTargetTypeId Id { get; set; }
    public required string Name { get; set; }
    public ICollection<Skill> Skills { get; set; } = [];
}
