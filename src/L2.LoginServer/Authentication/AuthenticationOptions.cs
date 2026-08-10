namespace L2.LoginServer.Authentication;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public bool RunPlayerIdentityMigrations { get; init; } = true;

    public string SessionCookieName { get; init; } = "l2.player_session";

    public int SessionIdleHours { get; init; } = 24;

    public int GameTicketLifetimeSeconds { get; init; } = 30;
}
