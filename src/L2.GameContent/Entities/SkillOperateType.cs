using L2.GameContent.Identifiers;

namespace L2.GameContent.Entities;

public sealed class SkillOperateType
{
    public SkillOperateTypeId Id { get; set; }
    public required string Name { get; set; }
    public ICollection<Skill> Skills { get; set; } = [];
}
