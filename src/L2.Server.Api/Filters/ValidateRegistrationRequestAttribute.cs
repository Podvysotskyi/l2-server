using L2.Server.Contracts;

namespace L2.Server.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ValidateRegistrationRequestAttribute : ValidateRequestAttribute<RegistrationRequest>
{
    protected override Dictionary<string, string[]> Validate(RegistrationRequest request) =>
        CredentialRequestValidator.Validate(request);
}
