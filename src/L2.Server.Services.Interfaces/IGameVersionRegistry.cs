using L2.Server.Contracts;

namespace L2.Server.Services.Interfaces;

public interface IGameVersionRegistry
{
    string DefaultKey { get; }
    IReadOnlyList<GameVersionSummary> GetEnabled();
    bool IsEnabled(string key);
    bool IsEnabled(string gameVersion, string gameServer);
    Task<IReadOnlyList<GameServerSummary>?> GetServersAsync(
        string gameVersion,
        CancellationToken cancellationToken = default);
}
