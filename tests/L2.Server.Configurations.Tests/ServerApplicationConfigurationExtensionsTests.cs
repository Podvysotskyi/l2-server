using L2.Server.Configurations;
using L2.Server.Context.Identifiers;
using L2.Server.Repositories;
using L2.Server.Repositories.Interfaces;
using L2.Server.Services;
using L2.Server.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace L2.Server.Configurations.Tests;

public sealed class ServerApplicationConfigurationExtensionsTests
{
    [Theory]
    [InlineData("GameVersions:Default", "unknown")]
    [InlineData("GameVersions:Enabled:0:Key", "unknown")]
    public void AddApiApplication_rejects_unknown_game_version_identifiers(string key, string value)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSql"] = "Host=localhost;Database=l2-server",
                ["GameVersions:Default"] = GameVersionIdentifiers.Interlude,
                ["GameVersions:Enabled:0:Key"] = GameVersionIdentifiers.Interlude,
                ["GameVersions:Enabled:0:DisplayName"] = "Interlude",
                ["GameVersions:Enabled:0:SortOrder"] = "30",
                ["GameVersions:Enabled:0:Servers:0:Key"] = "default",
                ["GameVersions:Enabled:0:Servers:0:DisplayName"] = "Default Server",
                ["GameVersions:Enabled:0:Servers:0:IsDefault"] = "true",
                ["GameVersions:Enabled:0:Servers:0:PublicUrl"] = "https://game.test",
                ["GameVersions:Enabled:0:Servers:0:HealthUrl"] = "https://health.test",
                [key] = value
            }).Build();
        var services = new ServiceCollection();
        services.AddApiApplication(configuration);

        using var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<GameVersionOptions>>().Value);
    }

    [Fact]
    public void AddApiApplication_registers_authentication_and_catalog_services()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSql"] = "Host=localhost;Database=l2-server",
                ["Authentication:SessionCookieName"] = "test.session",
                ["Authentication:SessionIdleHours"] = "12",
                ["Authentication:GameTicketLifetimeSeconds"] = "45",
                ["GameVersions:Default"] = GameVersionIdentifiers.Interlude,
                ["GameVersions:Enabled:0:Key"] = GameVersionIdentifiers.Interlude,
                ["GameVersions:Enabled:0:DisplayName"] = "Interlude",
                ["GameVersions:Enabled:0:SortOrder"] = "30",
                ["GameVersions:Enabled:0:Servers:0:Key"] = "default",
                ["GameVersions:Enabled:0:Servers:0:DisplayName"] = "Default Server",
                ["GameVersions:Enabled:0:Servers:0:IsDefault"] = "true",
                ["GameVersions:Enabled:0:Servers:0:PublicUrl"] = "https://game.test",
                ["GameVersions:Enabled:0:Servers:0:HealthUrl"] = "https://health.test"
            }).Build();
        var services = new ServiceCollection();

        services.AddApiApplication(configuration);

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IPlayerAuthenticationService) &&
            descriptor.ImplementationType == typeof(PlayerAuthenticationService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IGameVersionRegistry) &&
            descriptor.ImplementationType == typeof(GameVersionRegistry));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IGameSessionService));

        using var provider = services.BuildServiceProvider();
        Assert.Equal("test.session",
            provider.GetRequiredService<IOptions<PlayerSessionCookieOptions>>().Value.SessionCookieName);
        Assert.Equal(GameVersionIdentifiers.Interlude,
            provider.GetRequiredService<IOptions<GameVersionOptions>>().Value.Default);
    }
}
