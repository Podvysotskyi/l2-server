using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace L2.Shared;

public static class FoundationExtensions
{
    private const string ReadinessTag = "ready";

    public static WebApplicationBuilder AddL2Foundation(
        this WebApplicationBuilder builder,
        string serviceName,
        DependencyOptions? dependencies = null)
    {
        dependencies ??= new DependencyOptions();
        builder.Configuration.GetSection(DependencyOptions.SectionName).Bind(dependencies);
        var otlpEndpoint = GetOtlpEndpoint(builder.Configuration);

        builder.Logging.ClearProviders();
        builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);
        if (otlpEndpoint is not null)
        {
            builder.Logging.AddOpenTelemetry(options =>
                options.AddOtlpExporter(exporter => exporter.Endpoint = otlpEndpoint));

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(exporter => exporter.Endpoint = otlpEndpoint))
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(exporter => exporter.Endpoint = otlpEndpoint));
        }

        builder.Services.AddHttpClient();
        var healthChecks = builder.Services.AddHealthChecks();
        if (dependencies.PostgreSqlRequired)
        {
            healthChecks.AddCheck<PostgreSqlHealthCheck>("postgresql", tags: [ReadinessTag]);
        }

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
        var otlpEndpoint = GetOtlpEndpoint(builder.Configuration);
        builder.Logging.ClearProviders();
        builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);
        if (otlpEndpoint is not null)
        {
            builder.Logging.AddOpenTelemetry(options =>
                options.AddOtlpExporter(exporter => exporter.Endpoint = otlpEndpoint));
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing => tracing
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(exporter => exporter.Endpoint = otlpEndpoint))
                .WithMetrics(metrics => metrics
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(exporter => exporter.Endpoint = otlpEndpoint));
        }
        builder.Services.AddSingleton(new ServiceIdentity(serviceName));
        return builder;
    }

    public static string BuildVersion() =>
        Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "0.1.0-local";

    private static Uri? GetOtlpEndpoint(IConfiguration configuration)
    {
        var value = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        return string.IsNullOrWhiteSpace(value) ? null : new Uri(value, UriKind.Absolute);
    }
}

public sealed record ServiceIdentity(string Name);
