using L2.Server.Contracts;
using L2.Server.Api.Filters;
using L2.Server.Configurations;
using L2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace L2.Server.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthenticationController(
    IAntiforgery antiforgery,
    IPlayerAuthenticationService authentication,
    IOptions<PlayerSessionCookieOptions> cookieOptions,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet("csrf")]
    public ActionResult<CsrfTokenResponse> Csrf()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new CsrfTokenResponse(tokens.RequestToken ?? string.Empty));
    }

    [HttpPost("register")]
    [EnableRateLimiting("player-registration")]
    [ValidateRegistrationRequest]
    public async Task<IActionResult> Register(
        [FromBody] RegistrationRequest request,
        CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(HttpContext);
        var registration = await authentication.RegisterAsync(request, cancellationToken);
        return registration is null
            ? Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Account unavailable",
                Detail = "Choose another username or email address."
            })
            : StatusCode(StatusCodes.Status201Created, registration);
    }

    [HttpPost("login")]
    [EnableRateLimiting("player-login")]
    [ValidateLoginRequest]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(HttpContext);
        var issue = await authentication.LoginAsync(
            request,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);
        if (issue is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid credentials",
                Detail = "The email or password is incorrect."
            });
        }
        WriteSessionCookie(issue);
        return Ok(issue.Session);
    }

    [HttpGet("session")]
    public async Task<IActionResult> Session(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(cookieOptions.Value.SessionCookieName, out var token)) return Unauthorized();
        var lookup = await authentication.FindSessionAsync(token, cancellationToken);
        if (lookup is null)
        {
            DeleteSessionCookie();
            return Unauthorized();
        }
        if (lookup.Refreshed) WriteSessionCookie(new AuthenticationIssue(lookup.Session, token));
        return Ok(lookup.Session);
    }

    [HttpPost("game-ticket")]
    [EnableRateLimiting("game-ticket")]
    public async Task<IActionResult> GameTicket(CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(HttpContext);
        if (!Request.Cookies.TryGetValue(cookieOptions.Value.SessionCookieName, out var token)) return Unauthorized();
        var ticket = await authentication.CreateGameTicketAsync(token, cancellationToken);
        if (ticket is not null) return Ok(ticket);
        DeleteSessionCookie();
        return Unauthorized();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(HttpContext);
        if (Request.Cookies.TryGetValue(cookieOptions.Value.SessionCookieName, out var token))
        {
            await authentication.LogoutAsync(token, cancellationToken);
        }
        DeleteSessionCookie();
        return NoContent();
    }

    private void WriteSessionCookie(AuthenticationIssue issue) => Response.Cookies.Append(
        cookieOptions.Value.SessionCookieName,
        issue.Token,
        CookieOptions(issue.Session.ExpiresAt));

    private void DeleteSessionCookie() => Response.Cookies.Delete(
        cookieOptions.Value.SessionCookieName,
        CookieOptions(null));

    private CookieOptions CookieOptions(DateTimeOffset? expiresAt) => new()
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Strict,
        Secure = !environment.IsDevelopment() && !environment.IsEnvironment("Testing"),
        Path = "/",
        Expires = expiresAt
    };
}
