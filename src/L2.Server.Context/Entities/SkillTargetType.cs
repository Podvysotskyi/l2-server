using L2.Server.Context.Identifiers;

namespace L2.Server.Context.Entities;

public sealed class SkillTargetType
{
    public SkillTargetTypeId Id { get; set; }
    public required string Name { get; set; }
    public ICollection<Skill> Skills { get; set; } = [];
}
