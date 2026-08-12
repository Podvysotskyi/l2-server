using L2.Server.Services.Interfaces;
using Microsoft.Extensions.Hosting;

namespace L2.Server.Game;

public sealed class GameHostIdentityValidator(
    GameHostIdentity identity,
    IGameVersionRegistry versions) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!versions.IsEnabled(identity.GameVersion, identity.GameServer))
        {
            throw new InvalidOperationException(
                $"Game host '{identity.GameVersion}/{identity.GameServer}' is not enabled in the Server catalog.");
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
