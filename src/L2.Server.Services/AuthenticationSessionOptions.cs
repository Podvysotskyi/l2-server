namespace L2.Server.Services;

public sealed class AuthenticationSessionOptions
{
    public const string SectionName = "Authentication";

    public int SessionIdleHours { get; init; } = 24;
    public int GameTicketLifetimeSeconds { get; init; } = 30;
}
