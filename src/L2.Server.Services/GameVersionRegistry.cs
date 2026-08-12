using L2.Server.Contracts;
using L2.Server.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace L2.Server.Services;

public sealed class GameVersionRegistry(IOptions<GameVersionOptions> options) : IGameVersionRegistry
{
    private readonly GameVersionOptions options = options.Value;

    public string DefaultKey => options.Default;

    public IReadOnlyList<GameVersionSummary> GetEnabled() => options.Enabled
        .OrderBy(version => version.SortOrder)
        .Select(version => new GameVersionSummary(
            version.Key,
            version.DisplayName,
            version.Key == options.Default,
            $"/versions/{version.Key}/client-manifest.json"))
        .ToArray();

    public bool IsEnabled(string key) => options.Enabled.Any(
        version => string.Equals(version.Key, key, StringComparison.OrdinalIgnoreCase));
}
