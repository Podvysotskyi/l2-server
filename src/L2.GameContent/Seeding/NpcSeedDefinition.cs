using L2.GameContent.Identifiers;

namespace L2.GameContent.Seeding;

public sealed record NpcSeedDefinition(
    int Id,
    short Level,
    string? Name,
    NpcTypeId NpcTypeId,
    NpcRaceId? NpcRaceId,
    NpcSexId NpcSexId);
