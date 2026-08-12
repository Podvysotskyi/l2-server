using L2.Server.Contracts;
using L2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace L2.Server.Api.Controllers;

[Route("api/characters")]
public sealed class CharactersController(
    IGameSessionService sessions,
    IPlayerCharacterService characters) : GameControllerBase(sessions)
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var session = await AuthenticateAsync(cancellationToken);
        return session is null
            ? Unauthorized()
            : Ok(await characters.ListAsync(session.AccountId, session.GameVersion, cancellationToken));
    }

    [HttpGet("creation-options")]
    public async Task<IActionResult> CreationOptions(CancellationToken cancellationToken)
    {
        var session = await AuthenticateAsync(cancellationToken);
        return session is null
            ? Unauthorized()
            : Ok(await characters.GetCreationOptionsAsync(session.GameVersion, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CharacterCreationRequest request,
        CancellationToken cancellationToken)
    {
        var session = await AuthenticateAsync(cancellationToken);
        if (session is null) return Unauthorized();
        var result = await characters.CreateAsync(
            session.AccountId,
            session.GameVersion,
            request,
            cancellationToken);
        return result is { Succeeded: true, Character: not null }
            ? StatusCode(StatusCodes.Status201Created, result.Character)
            : DomainProblem(result.ErrorCode ?? "character_conflict");
    }

    [HttpPut("{characterId:guid}/selection")]
    public async Task<IActionResult> Select(Guid characterId, CancellationToken cancellationToken)
    {
        var session = await AuthenticateAsync(cancellationToken);
        if (session is null) return Unauthorized();
        var result = await Sessions.SelectCharacterAsync(session, characterId, cancellationToken);
        return result is { Succeeded: true, Character: not null }
            ? Ok(result.Character)
            : DomainProblem(result.ErrorCode ?? "character_conflict");
    }

    [HttpDelete("{characterId:guid}")]
    public async Task<IActionResult> Delete(Guid characterId, CancellationToken cancellationToken)
    {
        var session = await AuthenticateAsync(cancellationToken);
        if (session is null) return Unauthorized();
        var result = await Sessions.ScheduleDeletionAsync(session, characterId, cancellationToken);
        return result is { Succeeded: true, Character: not null }
            ? Ok(result.Character)
            : DomainProblem(result.ErrorCode ?? "character_conflict");
    }

    [HttpPost("{characterId:guid}/restore")]
    public async Task<IActionResult> Restore(Guid characterId, CancellationToken cancellationToken)
    {
        var session = await AuthenticateAsync(cancellationToken);
        if (session is null) return Unauthorized();
        var result = await characters.RestoreAsync(
            session.AccountId,
            session.GameVersion,
            characterId,
            cancellationToken);
        return result is { Succeeded: true, Character: not null }
            ? Ok(result.Character)
            : DomainProblem(result.ErrorCode ?? "character_conflict");
    }
}
