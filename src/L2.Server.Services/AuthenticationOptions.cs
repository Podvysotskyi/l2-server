namespace L2.Server.Services;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string SessionCookieName { get; init; } = "l2.player_session";

    public int SessionIdleHours { get; init; } = 24;

    public int GameTicketLifetimeSeconds { get; init; } = 30;
}
