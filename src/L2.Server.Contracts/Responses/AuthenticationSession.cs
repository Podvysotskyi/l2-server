namespace L2.Server.Contracts;

public sealed record AuthenticationSession(
    Guid AccountId,
    string Username,
    DateTimeOffset ExpiresAt);
