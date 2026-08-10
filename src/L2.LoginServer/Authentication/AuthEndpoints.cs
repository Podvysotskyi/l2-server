using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Options;

namespace L2.LoginServer.Authentication;

public static class AuthEndpoints
{
    public static IServiceCollection AddPlayerAuthentication(this IServiceCollection services, IConfiguration configuration)
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
            options.AddPolicy("player-login", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
            options.AddPolicy("player-registration", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(10),
                    QueueLimit = 0
                }));
            options.AddPolicy("game-ticket", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
        });
        return services;
    }

    public static IEndpointRouteBuilder MapPlayerAuthentication(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth");
        group.MapGet("/csrf", (HttpContext context, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            return Results.Ok(new { token = tokens.RequestToken });
        });
        group.MapPost("/register", Register).RequireRateLimiting("player-registration");
        group.MapPost("/login", Login).RequireRateLimiting("player-login");
        group.MapGet("/session", GetSession);
        group.MapPost("/game-ticket", CreateGameTicket).RequireRateLimiting("game-ticket");
        group.MapPost("/logout", Logout);
        return endpoints;
    }

    private static async Task<IResult> Register(
        RegistrationRequest request,
        HttpContext context,
        IAntiforgery antiforgery,
        PlayerAuthService auth,
        CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(context);
        var errors = CredentialRules.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var registration = await auth.RegisterAsync(request, cancellationToken);
        if (registration is null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Account unavailable",
                detail: "Choose another username or email address.");
        }

        return Results.Json(registration, statusCode: StatusCodes.Status201Created);
    }

    private static async Task<IResult> Login(
        CredentialRequest request,
        HttpContext context,
        IAntiforgery antiforgery,
        PlayerAuthService auth,
        IOptions<AuthenticationOptions> options,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(context);
        var errors = CredentialRules.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var issue = await auth.LoginAsync(request, Metadata(context), cancellationToken);
        if (issue is null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid credentials",
                detail: "The email or password is incorrect.");
        }

        WriteSessionCookie(context, issue, options.Value, environment);
        return Results.Ok(issue.Session);
    }

    private static async Task<IResult> GetSession(
        HttpContext context,
        PlayerAuthService auth,
        IOptions<AuthenticationOptions> options,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        if (!context.Request.Cookies.TryGetValue(options.Value.SessionCookieName, out var token))
        {
            return Results.Unauthorized();
        }

        var lookup = await auth.FindSessionAsync(token, cancellationToken);
        if (lookup is null)
        {
            DeleteSessionCookie(context, options.Value, environment);
            return Results.Unauthorized();
        }

        if (lookup.Refreshed)
        {
            WriteSessionCookie(context, new SessionIssue(lookup.Session, token), options.Value, environment);
        }

        return Results.Ok(lookup.Session);
    }

    private static async Task<IResult> Logout(
        HttpContext context,
        IAntiforgery antiforgery,
        PlayerAuthService auth,
        IOptions<AuthenticationOptions> options,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(context);
        if (context.Request.Cookies.TryGetValue(options.Value.SessionCookieName, out var token))
        {
            await auth.LogoutAsync(token, cancellationToken);
        }

        DeleteSessionCookie(context, options.Value, environment);
        return Results.NoContent();
    }

    private static async Task<IResult> CreateGameTicket(
        HttpContext context,
        IAntiforgery antiforgery,
        PlayerAuthService auth,
        IOptions<AuthenticationOptions> options,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(context);
        if (!context.Request.Cookies.TryGetValue(options.Value.SessionCookieName, out var sessionToken))
        {
            return Results.Unauthorized();
        }

        var ticket = await auth.CreateGameTicketAsync(sessionToken, cancellationToken);
        if (ticket is not null)
        {
            return Results.Ok(ticket);
        }

        DeleteSessionCookie(context, options.Value, environment);
        return Results.Unauthorized();
    }

    private static RequestMetadata Metadata(HttpContext context) => new(
        context.Connection.RemoteIpAddress?.ToString(),
        context.Request.Headers.UserAgent.ToString());

    private static void WriteSessionCookie(
        HttpContext context,
        SessionIssue issue,
        AuthenticationOptions options,
        IWebHostEnvironment environment) => context.Response.Cookies.Append(
            options.SessionCookieName,
            issue.Token,
            CookieOptions(issue.Session.ExpiresAt, environment));

    private static void DeleteSessionCookie(
        HttpContext context,
        AuthenticationOptions options,
        IWebHostEnvironment environment) => context.Response.Cookies.Delete(
            options.SessionCookieName,
            CookieOptions(null, environment));

    private static CookieOptions CookieOptions(DateTimeOffset? expiresAt, IWebHostEnvironment environment) => new()
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Strict,
        Secure = !environment.IsDevelopment() && !environment.IsEnvironment("Testing"),
        Path = "/",
        Expires = expiresAt
    };
}
