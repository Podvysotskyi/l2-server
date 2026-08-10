namespace L2.LoginServer.Authentication;

public sealed record CredentialRequest(string Email, string Password);

public sealed record RegistrationRequest(string Username, string Email, string Password, string PasswordConfirmation);

public sealed record AuthSession(Guid AccountId, string Username, DateTimeOffset ExpiresAt);

public sealed record AccountRegistration(Guid AccountId, string Username, string Email);

public sealed record AccountCredential(Guid AccountId, string Username, string PasswordHash);

public sealed record SessionIssue(AuthSession Session, string Token);

public sealed record SessionLookup(AuthSession Session, bool Refreshed);

public sealed record GameTicketIssue(string Ticket, DateTimeOffset ExpiresAt);

public sealed record RequestMetadata(string? IpAddress, string? UserAgent);
