using L2.Server.Context.Identifiers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("npc_races")]
public sealed class NpcRace
{
    [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public NpcRaceId Id { get; set; }
    [Column("name"), MaxLength(64)]
    public required string Name { get; set; }
    public ICollection<Npc> Npcs { get; set; } = [];
}
