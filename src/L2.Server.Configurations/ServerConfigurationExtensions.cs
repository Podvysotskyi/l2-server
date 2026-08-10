using L2.Server.Repositories;
using L2.Server.Repositories.Interfaces;
using L2.Server.Services;
using L2.Server.Services.Interfaces;
using L2.Server.Exceptions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace L2.Server.Configurations;

public static class ServerConfigurationExtensions
{
    public static WebApplicationBuilder AddServerFoundation(
        this WebApplicationBuilder builder,
        string serviceName)
    {
        builder.AddL2Foundation(serviceName);
        builder.Services.AddControllers();
        return builder;
    }

    public static IServiceCollection AddGameApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddServerPersistence(configuration);
        services.AddGameServices(configuration);
        return services;
    }

    public static IServiceCollection AddApiApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddServerPersistence(configuration);
        services.AddAuthenticationServices(configuration);
        services.AddGameServices(configuration);
        return services;
    }

    private static void AddGameServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddOptions<GameSessionOptions>()
            .Bind(configuration.GetSection(GameSessionOptions.SectionName))
            .Validate(options => options.ProtocolVersion > 0, "Protocol version must be positive.")
            .Validate(options => options.IdleTimeoutMinutes > 0, "Game session idle timeout must be positive.")
            .ValidateOnStart();
        services.AddSingleton<IGameSessionRepository, GameSessionRepository>();
        services.AddSingleton<IPlayerCharacterRepository, PlayerCharacterRepository>();
        services.AddSingleton<IGameSessionService, GameSessionService>();
        services.AddSingleton<IPlayerCharacterService, PlayerCharacterService>();
    }

    private static void AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionName));
        services.AddSingleton<IPasswordHasher<CredentialRecord>, PasswordHasher<CredentialRecord>>();
        services.AddSingleton<IPlayerAuthenticationRepository, PlayerAuthenticationRepository>();
        services.AddSingleton<IPlayerAuthenticationService, PlayerAuthenticationService>();
    }

    public static WebApplication MapServerFoundation(this WebApplication app)
    {
        app.MapL2Foundation();
        app.MapControllers();
        return app;
    }

    public static WebApplication UseServerExceptionHandling(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            try
            {
                await next(context);
            }
            catch (AntiforgeryValidationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "Invalid antiforgery token",
                    status = StatusCodes.Status400BadRequest
                });
            }
            catch (ServerRepositoryException exception)
            {
                context.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("ServerRepository")
                    .LogError(exception, "Server persistence is unavailable");
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "Service unavailable",
                    status = StatusCodes.Status503ServiceUnavailable
                });
            }
        });
        return app;
    }
}
