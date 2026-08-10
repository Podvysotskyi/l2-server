using L2.Server.Context.Identifiers;

namespace L2.Server.Context.Entities;

public sealed class PlayerRace
{
    public PlayerRaceId Id { get; set; }
    public required string Name { get; set; }
    public ICollection<PlayerClass> PlayerClasses { get; set; } = [];
    public ICollection<PlayerFace> PlayerFaces { get; set; } = [];
    public ICollection<PlayerHairStyle> PlayerHairStyles { get; set; } = [];
    public ICollection<PlayerHairColor> PlayerHairColors { get; set; } = [];
}
