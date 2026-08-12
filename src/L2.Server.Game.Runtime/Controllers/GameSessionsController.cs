using L2.Server.Contracts;
using L2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace L2.Server.Game.Controllers;

[Route("api/game-sessions")]
public sealed class GameSessionsController(IGameSessionService sessions) : GameControllerBase(sessions)
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateGameSessionRequest request,
        CancellationToken cancellationToken)
    {
        var issue = await Sessions.ExchangeAsync(request.Ticket, cancellationToken);
        if (issue is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid game ticket"
            });
        }

        return StatusCode(StatusCodes.Status201Created, new GameSessionCreated(
            issue.AccessToken,
            issue.Session.AccountId,
            issue.Session.Username,
            issue.Session.GameVersion,
            issue.Session.GameServer,
            issue.Session.ExpiresAt,
            issue.IdleTimeoutSeconds));
    }

    [HttpDelete("current")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken)
    {
        var session = await AuthenticateAsync(cancellationToken);
        if (session is null)
        {
            return Unauthorized();
        }

        await Sessions.RevokeAsync(session, cancellationToken);
        return NoContent();
    }
}
