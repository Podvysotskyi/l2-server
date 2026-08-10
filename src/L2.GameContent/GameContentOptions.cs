namespace L2.GameContent;

public sealed class GameContentOptions
{
    public const string SectionName = "GameContent";

    public bool RunMigrations { get; init; } = true;
    public bool SeedNpcLookups { get; init; } = true;
    public bool SeedPlayerLookups { get; init; } = true;
    public bool SeedPlayerClasses { get; init; }
    public bool SeedPlayerAppearances { get; init; }
    public bool SeedNpcs { get; init; }
    public bool SeedSkills { get; init; }
}
