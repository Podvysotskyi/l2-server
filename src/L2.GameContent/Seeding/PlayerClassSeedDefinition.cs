using L2.GameContent.Identifiers;

namespace L2.GameContent.Seeding;

public sealed record PlayerClassSeedDefinition(
    PlayerClassId Id,
    string Name,
    PlayerClassId? ParentClassId,
    bool IsMage = false,
    IReadOnlyList<PlayerClassRaceSeedDefinition>? Races = null)
{
    public IReadOnlyList<PlayerClassRaceSeedDefinition> AllowedRaces =>
        Races ?? PlayerClassSeedValues.ForCanonicalRace(Id);
}

public sealed record PlayerClassRaceSeedDefinition(
    PlayerRaceId Id,
    IReadOnlyList<PlayerSexId> AllowedSexIds);
