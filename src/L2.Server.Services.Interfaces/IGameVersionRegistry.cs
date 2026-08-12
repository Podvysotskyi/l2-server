using L2.Server.Contracts;

namespace L2.Server.Services.Interfaces;

public interface IGameVersionRegistry
{
    string DefaultKey { get; }
    IReadOnlyList<GameVersionSummary> GetEnabled();
    bool IsEnabled(string key);
}
