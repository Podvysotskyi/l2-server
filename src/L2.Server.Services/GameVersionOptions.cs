namespace L2.Server.Services;

public sealed class GameVersionOptions
{
    public const string SectionName = "GameVersions";

    public string Default { get; init; } = "interlude";
    public IReadOnlyList<GameVersionDefinition> Enabled { get; init; } = [];
}

public sealed record GameVersionDefinition(
    string Key,
    string DisplayName,
    int SortOrder,
    IReadOnlyList<GameServerDefinition> Servers);
