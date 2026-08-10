using L2.Server.Context.Identifiers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("skill_target_types")]
public sealed class SkillTargetType
{
    [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public SkillTargetTypeId Id { get; set; }
    [Column("name"), MaxLength(64)]
    public required string Name { get; set; }
    public ICollection<Skill> Skills { get; set; } = [];
}
