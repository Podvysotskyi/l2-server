using L2.Server.Configurations;

var builder = WebApplication.CreateBuilder(args)
    .AddServerFoundation("l2-server-api");
builder.Services.AddApiApplication(builder.Configuration);
builder.Services.AddPlayerAuthentication(builder.Configuration);
var app = builder.Build();
app.UseServerExceptionHandling();
app.UseRateLimiter();
app.MapServerFoundation();
app.Run();

namespace L2.Server.Api { public sealed class ApiServerMarker; }
