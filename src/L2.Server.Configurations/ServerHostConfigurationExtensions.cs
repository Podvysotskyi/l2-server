using System.Reflection;
using L2.Server.Contracts;
using L2.Server.Exceptions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace L2.Server.Configurations;

public static class ServerHostConfigurationExtensions
{
    private const string ReadinessTag = "ready";

    public static WebApplicationBuilder AddServerHost(
        this WebApplicationBuilder builder,
        string serviceName)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);
        builder.Services.AddHttpClient();

        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
        {
            if (origins.Length > 0)
            {
                policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            }
        }));

        builder.Services.AddSingleton(new ServiceIdentity(serviceName, BuildVersion()));
        return builder;
    }

    public static WebApplication MapServerHost(this WebApplication app)
    {
        app.UseCors();
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(ReadinessTag)
        });
        app.MapGet("/api/system/info", (ServiceIdentity identity, IWebHostEnvironment environment) =>
            new SystemInfo(identity.Name, identity.BuildVersion, environment.EnvironmentName));
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

    private static string BuildVersion() =>
        Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "0.1.0-local";
}
