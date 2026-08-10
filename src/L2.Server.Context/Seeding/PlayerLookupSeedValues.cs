using L2.Server.Context.Identifiers;

namespace L2.Server.Context.Seeding;

public static class PlayerLookupSeedValues
{
    public static IReadOnlyList<(PlayerRaceId Id, string Name)> Races { get; } =
    [
        (PlayerRaceId.Human, "Human"),
        (PlayerRaceId.Elf, "Elf"),
        (PlayerRaceId.DarkElf, "Dark Elf"),
        (PlayerRaceId.Orc, "Orc"),
        (PlayerRaceId.Dwarf, "Dwarf")
    ];

    public static IReadOnlyList<(PlayerSexId Id, string Name)> Sexes { get; } =
    [
        (PlayerSexId.Male, "Male"),
        (PlayerSexId.Female, "Female")
    ];
}
