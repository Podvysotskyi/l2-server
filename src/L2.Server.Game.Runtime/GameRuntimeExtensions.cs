using L2.Server.Configurations;
using L2.Server.Game.Sessions;
using L2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace L2.Server.Game;

public static class GameRuntimeExtensions
{
    public static WebApplicationBuilder AddGameRuntime(
        this WebApplicationBuilder builder,
        string serviceName,
        string gameVersion)
    {
        var gameServer = builder.Configuration[$"{GameHostOptions.SectionName}:ServerKey"] ?? "default";
        if (string.IsNullOrWhiteSpace(gameServer))
        {
            throw new InvalidOperationException("The Game host server key is required.");
        }
        builder.AddServerHost(serviceName);
        builder.Services.AddSingleton(new GameHostIdentity(
            gameVersion.Trim().ToLowerInvariant(),
            gameServer.Trim().ToLowerInvariant()));
        builder.Services.AddHostedService<GameHostIdentityValidator>();
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(GameRuntimeExtensions).Assembly);
        builder.Services.AddGameApplication(builder.Configuration);
        builder.Services.AddSingleton<GameSessionConnectionHandler>();
        return builder;
    }

    public static WebApplication MapGameRuntime(this WebApplication app)
    {
        app.UseServerExceptionHandling();
        app.UseWebSockets();
        app.MapServerHost();
        app.MapControllers();
        app.MapGameSessions();
        return app;
    }
}
