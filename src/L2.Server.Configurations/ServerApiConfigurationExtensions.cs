using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace L2.Server.Configurations;

public static class ServerApiConfigurationExtensions
{
    public static WebApplicationBuilder AddServerApi(
        this WebApplicationBuilder builder,
        string serviceName)
    {
        builder.AddServerHost(serviceName);
        builder.Services.AddControllers();
        return builder;
    }

    public static WebApplication MapServerApi(this WebApplication app)
    {
        app.MapServerHost();
        app.MapControllers();
        return app;
    }
}
