using L2.LoginServer.Authentication;
using Xunit;

namespace L2.Foundation.Tests;

public sealed class CredentialRulesTests
{
    [Theory]
    [InlineData("player@example.com", "password", true)]
    [InlineData("not-an-email", "password", false)]
    [InlineData("player@example.com", "short", false)]
    public void Login_credential_rules_validate_the_public_contract(string email, string password, bool valid)
    {
        var errors = CredentialRules.Validate(new CredentialRequest(email, password));
        Assert.Equal(valid, errors.Count == 0);
    }

    [Fact]
    public void Login_identifiers_are_normalized_case_insensitively()
    {
        Assert.Equal("PLAYER_1", CredentialRules.NormalizeUsername("Player_1"));
        Assert.Equal("PLAYER@EXAMPLE.COM", CredentialRules.NormalizeEmail(" player@example.com "));
    }

    [Theory]
    [InlineData("Passwor1")]
    [InlineData("Passwor!")]
    public void Registration_accepts_a_number_or_special_character(string password)
    {
        var errors = CredentialRules.Validate(new RegistrationRequest(
            "Player_1",
            "player@example.com",
            password,
            password));

        Assert.Empty(errors);
    }

    [Fact]
    public void Registration_rejects_passwords_without_a_number_or_special_character()
    {
        var errors = CredentialRules.Validate(new RegistrationRequest(
            "Player_1",
            "player@example.com",
            "OnlyLetters",
            "OnlyLetters"));

        Assert.Equal(
            "Password must contain at least one number or special character.",
            errors["password"].Single());
    }

    [Fact]
    public void Registration_requires_matching_passwords()
    {
        var errors = CredentialRules.Validate(new RegistrationRequest(
            "Player_1",
            "player@example.com",
            "Password1",
            "Different1"));

        Assert.Equal("Passwords do not match.", errors["passwordConfirmation"].Single());
    }
}
