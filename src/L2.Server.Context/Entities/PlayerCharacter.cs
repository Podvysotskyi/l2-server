using L2.Server.Context.Identifiers;

namespace L2.Server.Context.Entities;

public sealed class PlayerCharacter
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public int AccountSlot { get; set; }
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public PlayerRaceId PlayerRaceId { get; set; }
    public PlayerSexId PlayerSexId { get; set; }
    public PlayerClassId BaseClassId { get; set; }
    public PlayerClassId ActiveClassId { get; set; }
    public int FaceId { get; set; }
    public int HairStyleId { get; set; }
    public int HairColorId { get; set; }
    public short Level { get; set; } = 1;
    public long Experience { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeleteAfter { get; set; }
}
