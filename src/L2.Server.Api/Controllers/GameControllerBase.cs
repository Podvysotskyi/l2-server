using L2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace L2.Server.Api.Controllers;

[ApiController]
public abstract class GameControllerBase(IGameSessionService sessions) : ControllerBase
{
    protected IGameSessionService Sessions { get; } = sessions;

    protected async Task<GameSessionState?> AuthenticateAsync(CancellationToken cancellationToken)
    {
        var header = Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? await Sessions.AuthenticateAsync(header[7..].Trim(), cancellationToken)
            : null;
    }

    protected ObjectResult DomainProblem(string code)
    {
        var (status, title) = code switch
        {
            "character_not_found" => (StatusCodes.Status404NotFound, "Character not found"),
            "deletion_expired" => (StatusCodes.Status410Gone, "Character deletion expired"),
            "invalid_name" or "invalid_class_variant" or "invalid_appearance" =>
                (StatusCodes.Status422UnprocessableEntity, "Character request is invalid"),
            _ => (StatusCodes.Status409Conflict, "Character operation conflict")
        };
        var details = new ProblemDetails { Status = status, Title = title };
        details.Extensions["code"] = code;
        return StatusCode(status, details);
    }
}
