using L2.Server.Contracts;
using L2.Server.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace L2.Server.Services;

public sealed class GameVersionRegistry(
    IOptions<GameVersionOptions> options,
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache) : IGameVersionRegistry
{
    private readonly GameVersionOptions options = options.Value;
    private static readonly TimeSpan HealthTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromSeconds(5);

    public string DefaultKey => options.Default;

    public IReadOnlyList<GameVersionSummary> GetEnabled() => options.Enabled
        .OrderBy(version => version.SortOrder)
        .Select(version => new GameVersionSummary(
            version.Key,
            version.DisplayName,
            version.Key == options.Default,
            $"/{version.Key}/game",
            $"/versions/{version.Key}/current.json"))
        .ToArray();

    public bool IsEnabled(string key) => options.Enabled.Any(
        version => string.Equals(version.Key, key, StringComparison.OrdinalIgnoreCase));

    public bool IsEnabled(string gameVersion, string gameServer) => Find(gameVersion)?.Servers.Any(
        server => string.Equals(server.Key, gameServer, StringComparison.OrdinalIgnoreCase)) == true;

    public async Task<IReadOnlyList<GameServerSummary>?> GetServersAsync(
        string gameVersion,
        CancellationToken cancellationToken = default)
    {
        var version = Find(gameVersion);
        if (version is null) return null;
        return await Task.WhenAll(version.Servers.Select(server => SummaryAsync(server, cancellationToken)));
    }

    private GameVersionDefinition? Find(string key) => options.Enabled.SingleOrDefault(
        version => string.Equals(version.Key, key, StringComparison.OrdinalIgnoreCase));

    private async Task<GameServerSummary> SummaryAsync(
        GameServerDefinition server,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"game-server-health:{server.HealthUrl}";
        if (!cache.TryGetValue(cacheKey, out bool online))
        {
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(HealthTimeout);
                using var response = await httpClientFactory.CreateClient(nameof(GameVersionRegistry))
                    .GetAsync(server.HealthUrl, timeout.Token);
                online = response.IsSuccessStatusCode;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                online = false;
            }
            catch (HttpRequestException)
            {
                online = false;
            }
            cache.Set(cacheKey, online, CacheLifetime);
        }
        return new GameServerSummary(
            server.Key,
            server.DisplayName,
            server.IsDefault,
            server.PublicUrl.TrimEnd('/'),
            online ? "online" : "offline");
    }
}
