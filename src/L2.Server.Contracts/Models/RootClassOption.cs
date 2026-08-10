namespace L2.Server.Contracts;

public sealed record RootClassOption(
    int Id,
    string Name,
    bool IsMage,
    IReadOnlyList<RaceOption> AllowedRaces);
