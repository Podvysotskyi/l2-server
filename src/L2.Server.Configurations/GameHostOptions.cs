namespace L2.Server.Configurations;

public sealed class GameHostOptions
{
    public const string SectionName = "GameHost";

    public string ServerKey { get; init; } = "default";
}
