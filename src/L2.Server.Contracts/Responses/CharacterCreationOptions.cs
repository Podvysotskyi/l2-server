namespace L2.Server.Contracts;

public sealed record CharacterCreationOptions(
    int MaximumCharacters,
    IReadOnlyList<RootClassOption> Classes);
