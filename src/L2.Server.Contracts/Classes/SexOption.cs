namespace L2.Server.Contracts;

public sealed record SexOption(
    int Id,
    string Name,
    IReadOnlyList<AppearanceOption> Faces,
    IReadOnlyList<AppearanceOption> HairStyles,
    IReadOnlyList<AppearanceOption> HairColors);
