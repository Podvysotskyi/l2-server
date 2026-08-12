using L2.Server.Contracts;
using L2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace L2.Server.Api.Controllers;

[ApiController]
[Route("api/game-versions")]
public sealed class GameVersionsController(IGameVersionRegistry versions) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<GameVersionSummary>> List() => Ok(versions.GetEnabled());
}
