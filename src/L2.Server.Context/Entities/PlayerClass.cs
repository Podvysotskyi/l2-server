using L2.Server.Context.Identifiers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("player_classes")]
public sealed class PlayerClass
{
    [Column("id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public PlayerClassId Id { get; set; }
    [Column("player_sex_id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public PlayerSexId PlayerSexId { get; set; }
    [Column("player_race_id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public PlayerRaceId PlayerRaceId { get; set; }
    [Column("name"), MaxLength(64)]
    public required string Name { get; set; }
    [Column("is_mage")]
    public bool IsMage { get; set; }
    [Column("parent_class_id")]
    public PlayerClassId? ParentClassId { get; set; }
    public PlayerSex PlayerSex { get; set; } = null!;
    public PlayerRace PlayerRace { get; set; } = null!;
    public PlayerClass? ParentClass { get; set; }
    public ICollection<PlayerClass> ChildClasses { get; set; } = [];
}
