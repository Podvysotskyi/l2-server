using L2.Server.Contracts;

namespace L2.Server.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ValidateLoginRequestAttribute : ValidateRequestAttribute<LoginRequest>
{
    protected override Dictionary<string, string[]> Validate(LoginRequest request) =>
        CredentialRequestValidator.Validate(request);
}
