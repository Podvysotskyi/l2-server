using System.Net;
using L2.Server.Services;
using L2.Server.Context.Identifiers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace L2.Server.Services.Tests;

public sealed class GameVersionRegistryTests
{
    [Fact]
    public void Lists_configured_versions_in_order_with_client_manifests()
    {
        var registry = new GameVersionRegistry(Options.Create(new GameVersionOptions
        {
            Default = GameVersionIdentifiers.Interlude,
            Enabled =
            [
                Version(GameVersionIdentifiers.Interlude, "Interlude", 30),
                Version(GameVersionIdentifiers.C1, "Chronicle 1", 10),
                Version(GameVersionIdentifiers.C4, "Chronicle 4", 20)
            ]
        }), new StubHttpClientFactory(), new MemoryCache(new MemoryCacheOptions()));

        var versions = registry.GetEnabled();

        Assert.Equal([GameVersionIdentifiers.C1, GameVersionIdentifiers.C4, GameVersionIdentifiers.Interlude], versions.Select(version => version.Key));
        Assert.Equal($"/versions/{GameVersionIdentifiers.C1}/client-manifest.json", versions[0].ClientManifestPath);
        Assert.True(versions[2].IsDefault);
        Assert.True(registry.IsEnabled(GameVersionIdentifiers.C1.ToUpperInvariant()));
        Assert.True(registry.IsEnabled(GameVersionIdentifiers.Interlude, "default"));
    }

    [Fact]
    public async Task Reports_health_without_exposing_the_internal_health_url()
    {
        var registry = new GameVersionRegistry(Options.Create(new GameVersionOptions
        {
            Default = GameVersionIdentifiers.Interlude,
            Enabled = [Version(GameVersionIdentifiers.Interlude, "Interlude", 10)]
        }), new StubHttpClientFactory(HttpStatusCode.OK), new MemoryCache(new MemoryCacheOptions()));

        var servers = await registry.GetServersAsync(GameVersionIdentifiers.Interlude);

        var server = Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<L2.Server.Contracts.GameServerSummary>>(servers));
        Assert.Equal("online", server.Status);
        Assert.Equal("https://game.test", server.PublicUrl);
    }

    private static GameVersionDefinition Version(string key, string name, int order) =>
        new(key, name, order,
            [new GameServerDefinition("default", "Default Server", true, "https://game.test", "https://health.test")]);

    private sealed class StubHttpClientFactory(HttpStatusCode statusCode = HttpStatusCode.OK) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(new StubHandler(statusCode));
    }

    private sealed class StubHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(statusCode));
    }
}
