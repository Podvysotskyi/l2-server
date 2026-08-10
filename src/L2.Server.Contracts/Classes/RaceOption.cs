namespace L2.Server.Contracts;

public sealed record RaceOption(int Id, string Name, IReadOnlyList<SexOption> AllowedSexes);
