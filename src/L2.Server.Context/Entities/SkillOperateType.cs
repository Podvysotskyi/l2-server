using L2.Server.Context.Identifiers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("skill_operate_types")]
public sealed class SkillOperateType
{
    [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public SkillOperateTypeId Id { get; set; }
    [Column("name"), MaxLength(64)]
    public required string Name { get; set; }
    public ICollection<Skill> Skills { get; set; } = [];
}
