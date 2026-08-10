using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("skill_icons")]
public sealed class SkillIcon
{
    [Column("skill_id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int SkillId { get; set; }
    [Column("level"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short Level { get; set; }
    [Column("name"), MaxLength(64)]
    public required string Name { get; set; }
    public Skill Skill { get; set; } = null!;
}
