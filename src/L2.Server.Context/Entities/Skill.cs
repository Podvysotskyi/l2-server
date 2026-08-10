using L2.Server.Context.Identifiers;

namespace L2.Server.Context.Entities;

public sealed class Skill
{
    public int Id { get; set; }
    public short Levels { get; set; }
    public required string Name { get; set; }
    public SkillOperateTypeId? SkillOperateTypeId { get; set; }
    public SkillTargetTypeId? SkillTargetTypeId { get; set; }
    public SkillOperateType? SkillOperateType { get; set; }
    public SkillTargetType? SkillTargetType { get; set; }
    public ICollection<SkillIcon> SkillIcons { get; set; } = [];
}
