using System.Security.Cryptography;
using L2.Server.Contracts;
using L2.Server.Repositories.Interfaces;
using L2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace L2.Server.Services;

public sealed class PlayerAuthenticationService : IPlayerAuthenticationService
{
    private readonly IPlayerAuthenticationRepository repository;
    private readonly IPasswordHasher<CredentialRecord> passwordHasher;
    private readonly TimeProvider timeProvider;
    private readonly TimeSpan sessionLifetime;
    private readonly TimeSpan gameTicketLifetime;
    private readonly string fallbackPasswordHash;
    private readonly IGameVersionRegistry gameVersions;

    public PlayerAuthenticationService(
        IPlayerAuthenticationRepository repository,
        IPasswordHasher<CredentialRecord> passwordHasher,
        TimeProvider timeProvider,
        IGameVersionRegistry gameVersions,
        IOptions<AuthenticationSessionOptions> options)
    {
        this.repository = repository;
        this.passwordHasher = passwordHasher;
        this.timeProvider = timeProvider;
        this.gameVersions = gameVersions;
        sessionLifetime = TimeSpan.FromHours(options.Value.SessionIdleHours);
        gameTicketLifetime = TimeSpan.FromSeconds(options.Value.GameTicketLifetimeSeconds);
        fallbackPasswordHash = passwordHasher.HashPassword(
            new CredentialRecord(Guid.Empty, string.Empty, string.Empty),
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
    }

    public async Task<AccountRegistration?> RegisterAsync(
        RegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var accountId = Guid.NewGuid();
        var credential = new CredentialRecord(accountId, request.Username, string.Empty);
        var passwordHash = passwordHasher.HashPassword(credential, request.Password);
        var now = timeProvider.GetUtcNow();
        var created = await repository.CreateAccountAsync(
            accountId,
            request.Username,
            CredentialNormalizer.Username(request.Username),
            request.Email.Trim(),
            CredentialNormalizer.Email(request.Email),
            passwordHash,
            now,
            cancellationToken);
        return created ? new AccountRegistration(accountId, request.Username, request.Email.Trim()) : null;
    }

    public async Task<AuthenticationIssue?> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = CredentialNormalizer.Email(request.Email);
        var credential = await repository.FindCredentialAsync(normalizedEmail, cancellationToken);
        var verification = credential is null
            ? passwordHasher.VerifyHashedPassword(
                new CredentialRecord(Guid.Empty, string.Empty, fallbackPasswordHash),
                fallbackPasswordHash,
                request.Password)
            : passwordHasher.VerifyHashedPassword(credential, credential.PasswordHash, request.Password);

        var now = timeProvider.GetUtcNow();
        if (credential is null || verification == PasswordVerificationResult.Failed)
        {
            await repository.RecordFailedLoginAsync(credential?.AccountId, normalizedEmail,
                new RequestMetadata(ipAddress, userAgent), now, cancellationToken);
            return null;
        }

        var token = CreateToken();
        var expiresAt = now.Add(sessionLifetime);
        var replacementHash = verification == PasswordVerificationResult.SuccessRehashNeeded
            ? passwordHasher.HashPassword(credential, request.Password)
            : null;
        await repository.CreateLoginSessionAsync(
            credential,
            normalizedEmail,
            replacementHash,
            OpaqueToken.Hash(token),
            now,
            expiresAt,
            new RequestMetadata(ipAddress, userAgent),
            cancellationToken);
        return new AuthenticationIssue(new AuthenticationSession(
            credential.AccountId,
            credential.Username,
            expiresAt), token);
    }

    public async Task<L2.Server.Services.Interfaces.AuthenticationSessionLookup?> FindSessionAsync(
        string token,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var lookup = await repository.FindSessionAsync(
            OpaqueToken.Hash(token), now, now.Add(sessionLifetime), cancellationToken);
        return lookup is null
            ? null
            : new L2.Server.Services.Interfaces.AuthenticationSessionLookup(lookup.Session, lookup.Refreshed);
    }

    public Task LogoutAsync(string token, CancellationToken cancellationToken) =>
        repository.RevokeSessionAsync(OpaqueToken.Hash(token), timeProvider.GetUtcNow(), cancellationToken);

    public async Task<GameTicketIssue?> CreateGameTicketAsync(
        string sessionToken,
        CreateGameTicketRequest request,
        CancellationToken cancellationToken)
    {
        var gameVersion = request.GameVersion.Trim().ToLowerInvariant();
        var gameServer = request.GameServer.Trim().ToLowerInvariant();
        if (!gameVersions.IsEnabled(gameVersion, gameServer)) return null;
        var ticket = OpaqueToken.Create();
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.Add(gameTicketLifetime);
        var created = await repository.CreateGameTicketAsync(
            OpaqueToken.Hash(sessionToken),
            OpaqueToken.Hash(ticket),
            gameVersion,
            gameServer,
            now,
            expiresAt,
            cancellationToken);
        return created ? new GameTicketIssue(ticket, expiresAt, gameVersion, gameServer) : null;
    }

    private static string CreateToken() => OpaqueToken.Create();
}
