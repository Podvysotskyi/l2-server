using System.Security.Cryptography;
using L2.LoginServer.Data;
using L2.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace L2.LoginServer.Authentication;

public sealed class PlayerAuthService
{
    private readonly PlayerAuthRepository repository;
    private readonly IPasswordHasher<AccountCredential> passwordHasher;
    private readonly TimeProvider timeProvider;
    private readonly TimeSpan sessionLifetime;
    private readonly TimeSpan gameTicketLifetime;
    private readonly string fallbackPasswordHash;

    public PlayerAuthService(
        PlayerAuthRepository repository,
        IPasswordHasher<AccountCredential> passwordHasher,
        TimeProvider timeProvider,
        IOptions<AuthenticationOptions> options)
    {
        this.repository = repository;
        this.passwordHasher = passwordHasher;
        this.timeProvider = timeProvider;
        sessionLifetime = TimeSpan.FromHours(options.Value.SessionIdleHours);
        gameTicketLifetime = TimeSpan.FromSeconds(options.Value.GameTicketLifetimeSeconds);
        fallbackPasswordHash = passwordHasher.HashPassword(
            new AccountCredential(Guid.Empty, string.Empty, string.Empty),
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
    }

    public async Task<AccountRegistration?> RegisterAsync(
        RegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var accountId = Guid.NewGuid();
        var credential = new AccountCredential(accountId, request.Username, string.Empty);
        var passwordHash = passwordHasher.HashPassword(credential, request.Password);
        var now = timeProvider.GetUtcNow();
        var created = await repository.CreateAccountAsync(
            accountId,
            request.Username,
            CredentialRules.NormalizeUsername(request.Username),
            request.Email.Trim(),
            CredentialRules.NormalizeEmail(request.Email),
            passwordHash,
            now,
            cancellationToken);
        return created ? new AccountRegistration(accountId, request.Username, request.Email.Trim()) : null;
    }

    public async Task<SessionIssue?> LoginAsync(
        CredentialRequest request,
        RequestMetadata metadata,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = CredentialRules.NormalizeEmail(request.Email);
        var credential = await repository.FindCredentialAsync(normalizedEmail, cancellationToken);
        var verification = credential is null
            ? passwordHasher.VerifyHashedPassword(
                new AccountCredential(Guid.Empty, string.Empty, fallbackPasswordHash),
                fallbackPasswordHash,
                request.Password)
            : passwordHasher.VerifyHashedPassword(credential, credential.PasswordHash, request.Password);

        var now = timeProvider.GetUtcNow();
        if (credential is null || verification == PasswordVerificationResult.Failed)
        {
            await repository.RecordFailedLoginAsync(credential?.AccountId, normalizedEmail, metadata, now, cancellationToken);
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
            HashToken(token),
            now,
            expiresAt,
            metadata,
            cancellationToken);
        return new SessionIssue(new AuthSession(credential.AccountId, credential.Username, expiresAt), token);
    }

    public Task<SessionLookup?> FindSessionAsync(string token, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        return repository.FindSessionAsync(HashToken(token), now, now.Add(sessionLifetime), cancellationToken);
    }

    public Task LogoutAsync(string token, CancellationToken cancellationToken) =>
        repository.RevokeSessionAsync(HashToken(token), timeProvider.GetUtcNow(), cancellationToken);

    public async Task<GameTicketIssue?> CreateGameTicketAsync(string sessionToken, CancellationToken cancellationToken)
    {
        var ticket = GameSessionTicketToken.Create();
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.Add(gameTicketLifetime);
        var created = await repository.CreateGameTicketAsync(
            HashToken(sessionToken),
            GameSessionTicketToken.Hash(ticket),
            now,
            expiresAt,
            cancellationToken);
        return created ? new GameTicketIssue(ticket, expiresAt) : null;
    }

    private static string CreateToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        .TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');

    private static byte[] HashToken(string token) => SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
}
