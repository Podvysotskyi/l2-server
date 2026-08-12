using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace L2.Server.Game.Sessions;

public static class GameSessionEndpoints
{
    public static IEndpointRouteBuilder MapGameSessions(this IEndpointRouteBuilder endpoints)
    {
        endpoints.Map("/ws/game", context => context.RequestServices
            .GetRequiredService<GameSessionConnectionHandler>()
            .HandleAsync(context));
        return endpoints;
    }
}
