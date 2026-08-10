using L2.GameContent.Identifiers;

namespace L2.GameContent.Entities;

public sealed class PlayerFace
{
    public int Id { get; set; }
    public PlayerSexId PlayerSexId { get; set; }
    public PlayerRaceId PlayerRaceId { get; set; }
    public required string Name { get; set; }
    public PlayerSex PlayerSex { get; set; } = null!;
    public PlayerRace PlayerRace { get; set; } = null!;
}
