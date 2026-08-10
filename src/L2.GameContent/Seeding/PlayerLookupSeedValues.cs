using L2.GameContent.Identifiers;

namespace L2.GameContent.Seeding;

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
