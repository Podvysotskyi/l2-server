using L2.Server.Context.Identifiers;

namespace L2.Server.Context.Seeding;

public sealed record NpcSeedDefinition(
    int Id,
    short Level,
    string? Name,
    NpcTypeId NpcTypeId,
    NpcRaceId? NpcRaceId,
    NpcSexId NpcSexId);
