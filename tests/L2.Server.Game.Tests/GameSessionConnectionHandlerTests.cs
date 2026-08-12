using L2.Server.Configurations;
using L2.Server.Contracts;
using L2.Server.Game.Sessions;
using L2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace L2.Server.Game.Tests;

public sealed class GameSessionConnectionHandlerTests
{
    [Fact]
    public async Task HandleAsync_rejects_non_websocket_requests()
    {
        var handler = new GameSessionConnectionHandler(
            new StubGameSessionService(),
            new StubCharacterService(),
            Options.Create(new GameConnectionOptions()),
            new ConfigurationBuilder().Build(),
            new ServiceIdentity("game", "test"));
        var context = new DefaultHttpContext();

        await handler.HandleAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    private sealed class StubGameSessionService : IGameSessionService
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

    private sealed class StubCharacterService : IPlayerCharacterService
    {
        public Task<IReadOnlyList<PlayerCharacterSummary>> ListAsync(
            Guid accountId,
            string gameVersion,
            string gameServer,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PlayerCharacterSummary>>([]);

        public Task<CharacterCreationOptions> GetCreationOptionsAsync(
            string gameVersion,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new CharacterCreationOptions(0, []));

        public Task<CharacterOperationResult> CreateAsync(
            Guid accountId,
            string gameVersion,
            string gameServer,
            CharacterCreationRequest request,
            CancellationToken cancellationToken = default) => Task.FromResult(new CharacterOperationResult(false));

        public Task<CharacterOperationResult> SelectAsync(
            Guid accountId,
            string gameVersion,
            string gameServer,
            Guid characterId,
            CancellationToken cancellationToken = default) => Task.FromResult(new CharacterOperationResult(false));

        public Task<CharacterOperationResult> ScheduleDeletionAsync(
            Guid accountId,
            string gameVersion,
            string gameServer,
            Guid characterId,
            CancellationToken cancellationToken = default) => Task.FromResult(new CharacterOperationResult(false));

        public Task<CharacterOperationResult> RestoreAsync(
            Guid accountId,
            string gameVersion,
            string gameServer,
            Guid characterId,
            CancellationToken cancellationToken = default) => Task.FromResult(new CharacterOperationResult(false));
    }
}
