namespace L2.Server.Contracts;

public sealed record RegistrationRequest(
    string Username,
    string Email,
    string Password,
    string PasswordConfirmation);
