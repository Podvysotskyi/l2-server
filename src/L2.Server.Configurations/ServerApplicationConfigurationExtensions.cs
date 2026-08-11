using L2.Server.Repositories;
using L2.Server.Repositories.Interfaces;
using L2.Server.Services;
using L2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace L2.Server.Configurations;

public static class ServerApplicationConfigurationExtensions
{
    public static IServiceCollection AddGameApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddServerPersistence(configuration);
        services.AddGameConnectionOptions(configuration);
        services.AddGameServices(configuration);
        return services;
    }

    public static IServiceCollection AddApiApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddServerPersistence(configuration);
        services.AddServerApiSecurity(configuration);
        services.AddGameConnectionOptions(configuration);
        services.AddAuthenticationServices(configuration);
        services.AddGameServices(configuration);
        return services;
    }

    private static void AddGameServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddTimeProvider();
        services.AddOptions<GameSessionOptions>()
            .Bind(configuration.GetSection(GameSessionOptions.SectionName))
            .Validate(options => options.IdleTimeoutMinutes > 0,
                "Game session idle timeout must be positive.")
            .ValidateOnStart();
        services.AddOptions<PlayerCharacterOptions>()
            .Bind(configuration.GetSection(PlayerCharacterOptions.SectionName))
            .Validate(options => options.MaximumCharactersPerAccount > 0,
                "Character limit must be positive.")
            .Validate(options => options.MinimumNameLength > 0 &&
                options.MaximumNameLength >= options.MinimumNameLength &&
                options.MaximumNameLength <= 16,
                "Character name limits must be between 1 and 16.")
            .Validate(options => options.DeletionDelayDays > 0,
                "Character deletion delay must be positive.")
            .ValidateOnStart();
        services.AddSingleton<IGameSessionRepository, GameSessionRepository>();
        services.AddSingleton<IPlayerCharacterRepository, PlayerCharacterRepository>();
        services.AddSingleton<IGameSessionService, GameSessionService>();
        services.AddSingleton<ICharacterCreationContentProvider, MockCharacterCreationContentProvider>();
        services.AddSingleton<IPlayerCharacterService, PlayerCharacterService>();
    }

    private static void AddAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.TryAddTimeProvider();
        services.AddOptions<AuthenticationSessionOptions>()
            .Bind(configuration.GetSection(AuthenticationSessionOptions.SectionName))
            .Validate(options => options.SessionIdleHours > 0,
                "Authentication session lifetime must be positive.")
            .Validate(options => options.GameTicketLifetimeSeconds > 0,
                "Game ticket lifetime must be positive.")
            .ValidateOnStart();
        services.AddSingleton<IPasswordHasher<CredentialRecord>, PasswordHasher<CredentialRecord>>();
        services.AddSingleton<IPlayerAuthenticationRepository, PlayerAuthenticationRepository>();
        services.AddSingleton<IPlayerAuthenticationService, PlayerAuthenticationService>();
    }

    private static void AddGameConnectionOptions(
        this IServiceCollection services,
        IConfiguration configuration) => services.AddOptions<GameConnectionOptions>()
            .Bind(configuration.GetSection(GameConnectionOptions.SectionName))
            .Validate(options => options.ProtocolVersion > 0, "Protocol version must be positive.")
            .Validate(options => options.AuthenticationTimeoutSeconds > 0,
                "Game authentication timeout must be positive.")
            .ValidateOnStart();

    private static void TryAddTimeProvider(this IServiceCollection services)
    {
        if (!services.Any(descriptor => descriptor.ServiceType == typeof(TimeProvider)))
        {
            services.AddSingleton(TimeProvider.System);
        }
    }
}
