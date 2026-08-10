namespace L2.Server.Contracts;

public sealed record GameSessionCreated(
    string AccessToken,
    Guid AccountId,
    string Username,
    DateTimeOffset ExpiresAt,
    int IdleTimeoutSeconds);
