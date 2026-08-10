namespace L2.Server.Repositories.Interfaces;

public sealed record CredentialRecord(Guid AccountId, string Username, string PasswordHash);
