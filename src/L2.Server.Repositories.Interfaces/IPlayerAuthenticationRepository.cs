using L2.Server.Contracts;

namespace L2.Server.Repositories.Interfaces;

public interface IPlayerAuthenticationRepository
{
    Task<bool> CreateAccountAsync(
        Guid accountId,
        string username,
        string normalizedUsername,
        string email,
        string normalizedEmail,
        string passwordHash,
        DateTimeOffset now,
        CancellationToken cancellationToken);
    Task<CredentialRecord?> FindCredentialAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task CreateLoginSessionAsync(
        CredentialRecord credential,
        string normalizedEmail,
        string gameVersion,
        string? replacementPasswordHash,
        byte[] tokenHash,
        DateTimeOffset now,
        DateTimeOffset expiresAt,
        RequestMetadata metadata,
        CancellationToken cancellationToken);
    Task RecordFailedLoginAsync(
        Guid? accountId,
        string normalizedEmail,
        string gameVersion,
        RequestMetadata metadata,
        DateTimeOffset now,
        CancellationToken cancellationToken);
    Task<AuthenticationSessionRecord?> FindSessionAsync(
        byte[] tokenHash,
        DateTimeOffset now,
        DateTimeOffset refreshedExpiry,
        CancellationToken cancellationToken);
    Task<bool> CreateGameTicketAsync(
        byte[] sessionTokenHash,
        byte[] ticketTokenHash,
        DateTimeOffset now,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken);
    Task RevokeSessionAsync(byte[] tokenHash, DateTimeOffset now, CancellationToken cancellationToken);
}
