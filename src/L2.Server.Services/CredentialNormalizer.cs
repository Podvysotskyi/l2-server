namespace L2.Server.Services;

internal static class CredentialNormalizer
{
    public static string Username(string username) => username.ToUpperInvariant();

    public static string Email(string email) => email.Trim().ToUpperInvariant();
}
