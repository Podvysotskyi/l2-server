using System.Reflection;
using L2.Server.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace L2.Server.Configurations;

public static class FoundationExtensions
{
    private const string ReadinessTag = "ready";

    public static WebApplicationBuilder AddL2Foundation(
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

        builder.Services.AddSingleton(new ServiceIdentity(serviceName));
        return builder;
    }

    public static WebApplication MapL2Foundation(this WebApplication app)
    {
        app.UseCors();
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(ReadinessTag)
        });
        app.MapGet("/api/system/info", (ServiceIdentity identity, IWebHostEnvironment environment) =>
            new SystemInfo(identity.Name, BuildVersion(), environment.EnvironmentName));
        return app;
    }

    public static IHostApplicationBuilder AddL2WorkerFoundation(
        this IHostApplicationBuilder builder,
        string serviceName)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);
        builder.Services.AddSingleton(new ServiceIdentity(serviceName));
        return builder;
    }

    public static string BuildVersion() =>
        Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "0.1.0-local";

}

public sealed record ServiceIdentity(string Name);
