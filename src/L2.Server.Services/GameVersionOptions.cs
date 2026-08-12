namespace L2.Server.Services;

public sealed class GameVersionOptions
{
    public const string SectionName = "GameVersions";

    public string Default { get; init; } = "interlude";
    public IReadOnlyList<GameVersionDefinition> Enabled { get; init; } =
    [
        new("c1", "Chronicle 1", 10),
        new("c4", "Chronicle 4", 20),
        new("interlude", "Interlude", 30)
    ];
}

public sealed record GameVersionDefinition(string Key, string DisplayName, int SortOrder);
