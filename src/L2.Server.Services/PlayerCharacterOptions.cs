namespace L2.Server.Services;

public sealed class PlayerCharacterOptions
{
    public const string SectionName = "PlayerCharacters";

    public int MaximumCharactersPerAccount { get; init; } = 7;
    public int MinimumNameLength { get; init; } = 2;
    public int MaximumNameLength { get; init; } = 16;
    public int DeletionDelayDays { get; init; } = 7;
}
