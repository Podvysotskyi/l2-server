namespace L2.PlayerCharacters;

public sealed class PlayerCharacterOptions
{
    public const string SectionName = "PlayerCharacters";
    public bool RunMigrations { get; init; } = true;
    public int MaximumCharactersPerAccount { get; init; } = 7;
    public int MinimumNameLength { get; init; } = 2;
    public int MaximumNameLength { get; init; } = 16;
    public int DeletionDelayDays { get; init; } = 7;
}
