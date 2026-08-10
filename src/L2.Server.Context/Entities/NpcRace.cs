using L2.Server.Context.Identifiers;

namespace L2.Server.Context.Entities;

public sealed class NpcRace
{
    public NpcRaceId Id { get; set; }
    public required string Name { get; set; }
    public ICollection<Npc> Npcs { get; set; } = [];
}
