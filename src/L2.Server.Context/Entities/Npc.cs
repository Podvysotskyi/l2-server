using L2.Server.Context.Identifiers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("npcs")]
public sealed class Npc
{
    [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    [Column("level")]
    public short Level { get; set; }
    [Column("name"), MaxLength(100)]
    public string? Name { get; set; }
    [Column("npc_type_id")]
    public NpcTypeId NpcTypeId { get; set; }
    [Column("npc_race_id")]
    public NpcRaceId? NpcRaceId { get; set; }
    [Column("npc_sex_id")]
    public NpcSexId NpcSexId { get; set; }
    public NpcType NpcType { get; set; } = null!;
    public NpcRace? NpcRace { get; set; }
    public NpcSex NpcSex { get; set; } = null!;
}
