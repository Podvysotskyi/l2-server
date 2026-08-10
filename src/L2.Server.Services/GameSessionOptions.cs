namespace L2.Server.Services;

public sealed class GameSessionOptions
{
    public const string SectionName = "GameSession";
    public int IdleTimeoutMinutes { get; init; } = 30;
}
