using L2.Server.Context.Identifiers;

namespace L2.Server.Context.Entities;

public sealed class PlayerClass
{
    public PlayerClassId Id { get; set; }
    public PlayerSexId PlayerSexId { get; set; }
    public PlayerRaceId PlayerRaceId { get; set; }
    public required string Name { get; set; }
    public bool IsMage { get; set; }
    public PlayerClassId? ParentClassId { get; set; }
    public PlayerSex PlayerSex { get; set; } = null!;
    public PlayerRace PlayerRace { get; set; } = null!;
    public PlayerClass? ParentClass { get; set; }
    public ICollection<PlayerClass> ChildClasses { get; set; } = [];
}
