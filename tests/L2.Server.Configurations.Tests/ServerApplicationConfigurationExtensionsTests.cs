using L2.Server.Configurations;
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
    [Fact]
    public void AddApiApplication_registers_layer_abstractions_and_options()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSql"] = "Host=localhost;Database=l2-server",
                ["Authentication:SessionCookieName"] = "test.session",
                ["Authentication:SessionIdleHours"] = "12",
                ["Authentication:GameTicketLifetimeSeconds"] = "45",
                ["GameSession:ProtocolVersion"] = "2",
                ["GameSession:AuthenticationTimeoutSeconds"] = "4",
                ["GameSession:IdleTimeoutMinutes"] = "20",
                ["PlayerCharacters:MaximumCharactersPerAccount"] = "5",
                ["PlayerCharacters:MinimumNameLength"] = "2",
                ["PlayerCharacters:MaximumNameLength"] = "16",
                ["PlayerCharacters:DeletionDelayDays"] = "7"
            }).Build();
        var services = new ServiceCollection();

        services.AddApiApplication(configuration);

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IPlayerAuthenticationService) &&
            descriptor.ImplementationType == typeof(PlayerAuthenticationService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IGameSessionService) &&
            descriptor.ImplementationType == typeof(GameSessionService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IPlayerCharacterRepository) &&
            descriptor.ImplementationType == typeof(PlayerCharacterRepository));

        using var provider = services.BuildServiceProvider();
        Assert.Equal("test.session",
            provider.GetRequiredService<IOptions<PlayerSessionCookieOptions>>().Value.SessionCookieName);
        Assert.Equal(20,
            provider.GetRequiredService<IOptions<GameSessionOptions>>().Value.IdleTimeoutMinutes);
        Assert.Equal(5,
            provider.GetRequiredService<IOptions<PlayerCharacterOptions>>().Value.MaximumCharactersPerAccount);
    }
}
