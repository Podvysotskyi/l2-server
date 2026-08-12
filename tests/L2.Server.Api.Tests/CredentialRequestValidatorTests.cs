using L2.Server.Api.Filters;
using L2.Server.Contracts;

namespace L2.Server.Api.Tests;

public sealed class CredentialRequestValidatorTests
{
    [Fact]
    public void Login_rejects_invalid_email_and_short_password()
    {
        var errors = CredentialRequestValidator.Validate(new LoginRequest("invalid", "short", "interlude"));

        Assert.Contains("email", errors.Keys);
        Assert.Contains("password", errors.Keys);
    }

    [Fact]
    public void Login_requires_a_game_version()
    {
        var errors = CredentialRequestValidator.Validate(new LoginRequest(
            "player@example.com", "password1", ""));

        Assert.Contains("gameVersion", errors.Keys);
    }

    [Fact]
    public void Registration_accepts_valid_credentials()
    {
        var errors = CredentialRequestValidator.Validate(new RegistrationRequest(
            "Player_1",
            "player@example.com",
            "password1",
            "password1"));

        Assert.Empty(errors);
    }

    [Fact]
    public void Registration_rejects_confirmation_mismatch()
    {
        var errors = CredentialRequestValidator.Validate(new RegistrationRequest(
            "Player_1",
            "player@example.com",
            "password1",
            "password2"));

        Assert.Contains("passwordConfirmation", errors.Keys);
    }
}
