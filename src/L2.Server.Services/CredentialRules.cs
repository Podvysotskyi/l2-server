using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using L2.Server.Contracts;

namespace L2.Server.Services;

public static partial class CredentialRules
{
    public const int MinimumPasswordLength = 8;
    public const int MaximumPasswordLength = 128;
    public const int MaximumEmailLength = 254;

    [GeneratedRegex("^[A-Za-z0-9_]{3,24}$", RegexOptions.CultureInvariant)]
    private static partial Regex UsernamePattern();

    public static Dictionary<string, string[]> Validate(LoginRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        var email = request.Email?.Trim() ?? string.Empty;
        if (email.Length > MaximumEmailLength || !new EmailAddressAttribute().IsValid(email))
        {
            errors["email"] = ["Enter a valid email address."];
        }

        var passwordLength = (request.Password ?? string.Empty).EnumerateRunes().Count();
        if (passwordLength is < MinimumPasswordLength or > MaximumPasswordLength)
        {
            errors["password"] = ["Password must be 8–128 characters."];
        }

        return errors;
    }

    public static Dictionary<string, string[]> Validate(RegistrationRequest request)
    {
        var errors = Validate(new LoginRequest(request.Email, request.Password));
        if (!UsernamePattern().IsMatch(request.Username ?? string.Empty))
        {
            errors["username"] = ["Username must be 3–24 characters and contain only letters, numbers, or underscores."];
        }

        if (!errors.ContainsKey("password") &&
            !ContainsNumberOrSpecialCharacter(request.Password ?? string.Empty))
        {
            errors["password"] = ["Password must contain at least one number or special character."];
        }

        if (!string.Equals(request.Password, request.PasswordConfirmation, StringComparison.Ordinal))
        {
            errors["passwordConfirmation"] = ["Passwords do not match."];
        }

        return errors;
    }

    public static string NormalizeUsername(string username) => username.ToUpperInvariant();

    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private static bool ContainsNumberOrSpecialCharacter(string password) => password
        .EnumerateRunes()
        .Any(rune => Rune.IsDigit(rune) || (!Rune.IsLetterOrDigit(rune) && !Rune.IsWhiteSpace(rune)));
}
