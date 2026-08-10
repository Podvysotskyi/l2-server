using L2.GameContent.Identifiers;

namespace L2.GameContent.Seeding;

public sealed record PlayerAppearanceSeedDefinition(
    int Id,
    string Name,
    PlayerSexId PlayerSexId,
    PlayerRaceId PlayerRaceId);

public static class PlayerAppearanceSeedValues
{
    public static IReadOnlyList<PlayerAppearanceSeedDefinition> Faces { get; } = Build(3, 3);
    public static IReadOnlyList<PlayerAppearanceSeedDefinition> HairStyles { get; } = Build(5, 7);
    public static IReadOnlyList<PlayerAppearanceSeedDefinition> HairColors { get; } = Build(4, 4);

    private static IReadOnlyList<PlayerAppearanceSeedDefinition> Build(int maleCount, int femaleCount) =>
        Enum.GetValues<PlayerRaceId>()
            .SelectMany(raceId => Enum.GetValues<PlayerSexId>().SelectMany(sexId =>
                Enumerable.Range(0, sexId == PlayerSexId.Male ? maleCount : femaleCount)
                    .Select(id => new PlayerAppearanceSeedDefinition(
                        id,
                        $"Option {id + 1}",
                        sexId,
                        raceId))))
            .ToArray();
}
