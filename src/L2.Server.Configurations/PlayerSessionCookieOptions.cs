namespace L2.Server.Configurations;

public sealed class PlayerSessionCookieOptions
{
    public const string SectionName = "Authentication";

    public string SessionCookieName { get; init; } = "l2.player_session";
}
