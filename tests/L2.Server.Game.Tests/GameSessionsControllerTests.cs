using L2.Server.Game.Controllers;
using L2.Server.Contracts;
using L2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace L2.Server.Game.Tests;

public sealed class GameSessionsControllerTests
{
    [Fact]
    public async Task Create_returns_unauthorized_when_ticket_is_rejected()
    {
        var controller = new GameSessionsController(new RejectingGameSessionService());

        var result = await controller.Create(new CreateGameSessionRequest("invalid"), CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    private sealed class RejectingGameSessionService : IGameSessionService
    {
        public Task<GameSessionIssue?> ExchangeAsync(string ticket, CancellationToken cancellationToken) =>
            Task.FromResult<GameSessionIssue?>(null);

        public Task<GameSessionState?> AuthenticateAsync(
            string accessToken,
            CancellationToken cancellationToken) => Task.FromResult<GameSessionState?>(null);

        public Task<CharacterOperationResult> SelectCharacterAsync(
            GameSessionState session,
            Guid characterId,
            CancellationToken cancellationToken) => Task.FromResult(new CharacterOperationResult(false));

        public Task<CharacterOperationResult> ScheduleDeletionAsync(
            GameSessionState session,
            Guid characterId,
            CancellationToken cancellationToken) => Task.FromResult(new CharacterOperationResult(false));

        public Task RevokeAsync(GameSessionState session, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
