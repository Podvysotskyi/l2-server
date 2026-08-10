using L2.LoginServer.Authentication;
using L2.LoginServer.Data;
using L2.PlayerIdentity;
using L2.Shared;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args)
    .AddL2Foundation("l2-login-server");
builder.Services.AddPlayerIdentityPersistence(builder.Configuration);
builder.Services.AddHealthChecks().AddPlayerIdentityMigrationHealthCheck();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<IPasswordHasher<AccountCredential>, PasswordHasher<AccountCredential>>();
builder.Services.AddSingleton<PlayerAuthRepository>();
builder.Services.AddSingleton<PlayerAuthService>();
builder.Services.AddHostedService<PlayerIdentityMigrator>();
builder.Services.AddPlayerAuthentication(builder.Configuration);
var app = builder.Build();
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
    catch (PlayerIdentityPersistenceException exception)
    {
        context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("PlayerAuthentication")
            .LogError(exception, "Player authentication persistence is unavailable");
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsJsonAsync(new
        {
            title = "Authentication service unavailable",
            status = StatusCodes.Status503ServiceUnavailable
        });
    }
});
app.UseRateLimiter();
app.MapL2Foundation();
app.MapPlayerAuthentication();
app.Run();

namespace L2.LoginServer { public sealed class LoginServerMarker; }
