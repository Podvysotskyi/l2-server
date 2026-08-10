namespace L2.Server.Configurations;

public sealed class GameConnectionOptions
{
    public const string SectionName = "GameSession";

    public int ProtocolVersion { get; init; } = 2;
    public int AuthenticationTimeoutSeconds { get; init; } = 5;
}
