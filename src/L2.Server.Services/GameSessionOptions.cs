namespace L2.Server.Services;

public sealed class GameSessionOptions
{
    public const string SectionName = "GameSession";
    public int ProtocolVersion { get; init; } = 2;
    public int AuthenticationTimeoutSeconds { get; init; } = 5;
    public int IdleTimeoutMinutes { get; init; } = 30;
}
