using L2.Server.Services;
using Microsoft.Extensions.Options;

namespace L2.Server.Services.Tests;

public sealed class GameVersionRegistryTests
{
    [Fact]
    public void Lists_configured_versions_in_order_with_client_manifests()
    {
        var registry = new GameVersionRegistry(Options.Create(new GameVersionOptions
        {
            Default = "interlude",
            Enabled =
            [
                new("interlude", "Interlude", 30),
                new("c1", "Chronicle 1", 10),
                new("c4", "Chronicle 4", 20)
            ]
        }));

        var versions = registry.GetEnabled();

        Assert.Equal(["c1", "c4", "interlude"], versions.Select(version => version.Key));
        Assert.Equal("/versions/c1/client-manifest.json", versions[0].ClientManifestPath);
        Assert.True(versions[2].IsDefault);
        Assert.True(registry.IsEnabled("C1"));
    }
}
