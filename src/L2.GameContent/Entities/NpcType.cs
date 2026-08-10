using L2.GameContent.Identifiers;

namespace L2.GameContent.Entities;

public sealed class NpcType
{
    public NpcTypeId Id { get; set; }
    public required string Name { get; set; }
    public ICollection<Npc> Npcs { get; set; } = [];
}
