using L2.Server.Context.Identifiers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("skills")]
public sealed class Skill
{
    [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    [Column("levels")]
    public short Levels { get; set; }
    [Column("name"), MaxLength(100)]
    public required string Name { get; set; }
    [Column("skill_operate_type_id")]
    public SkillOperateTypeId? SkillOperateTypeId { get; set; }
    [Column("skill_target_type_id")]
    public SkillTargetTypeId? SkillTargetTypeId { get; set; }
    public SkillOperateType? SkillOperateType { get; set; }
    public SkillTargetType? SkillTargetType { get; set; }
    public ICollection<SkillIcon> SkillIcons { get; set; } = [];
}
