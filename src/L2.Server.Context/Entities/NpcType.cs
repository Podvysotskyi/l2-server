using L2.Server.Context.Identifiers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("npc_types")]
public sealed class NpcType
{
    [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public NpcTypeId Id { get; set; }
    [Column("name"), MaxLength(64)]
    public required string Name { get; set; }
    public ICollection<Npc> Npcs { get; set; } = [];
}
