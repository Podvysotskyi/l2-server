using System.Threading.RateLimiting;
using L2.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace L2.Server.Configurations;

public static class AuthenticationConfigurationExtensions
{
    public static IServiceCollection AddPlayerAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionName));
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "l2.csrf";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            AddPolicy(options, "player-login", 10, TimeSpan.FromMinutes(1));
            AddPolicy(options, "player-registration", 5, TimeSpan.FromMinutes(10));
            AddPolicy(options, "game-ticket", 20, TimeSpan.FromMinutes(1));
        });
        return services;
    }

    private static void AddPolicy(
        Microsoft.AspNetCore.RateLimiting.RateLimiterOptions options,
        string name,
        int permitLimit,
        TimeSpan window) => options.AddPolicy(name, context =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = window,
                    QueueLimit = 0
                }));
}
