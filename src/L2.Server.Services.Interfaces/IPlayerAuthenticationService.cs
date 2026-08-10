using L2.Server.Contracts;

namespace L2.Server.Services.Interfaces;

public interface IPlayerAuthenticationService
{
    Task<AccountRegistration?> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken);
    Task<AuthenticationIssue?> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);
    Task<AuthenticationSessionLookup?> FindSessionAsync(string token, CancellationToken cancellationToken);
    Task LogoutAsync(string token, CancellationToken cancellationToken);
    Task<GameTicketIssue?> CreateGameTicketAsync(string sessionToken, CancellationToken cancellationToken);
}
