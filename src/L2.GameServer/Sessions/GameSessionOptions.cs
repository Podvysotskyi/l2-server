namespace L2.GameServer.Sessions;

public sealed class GameSessionOptions
{
    public const string SectionName = "GameSession";

    public int ProtocolVersion { get; init; } = 1;

    public int AuthenticationTimeoutSeconds { get; init; } = 5;
}
