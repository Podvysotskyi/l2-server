using L2.GameContent.Identifiers;

namespace L2.GameContent.Entities;

public sealed class PlayerSex
{
    public PlayerSexId Id { get; set; }
    public required string Name { get; set; }
    public ICollection<PlayerClass> PlayerClasses { get; set; } = [];
    public ICollection<PlayerFace> PlayerFaces { get; set; } = [];
    public ICollection<PlayerHairStyle> PlayerHairStyles { get; set; } = [];
    public ICollection<PlayerHairColor> PlayerHairColors { get; set; } = [];
}
