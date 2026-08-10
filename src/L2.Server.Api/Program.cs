using L2.Server.Configurations;

var builder = WebApplication.CreateBuilder(args)
    .AddServerApi("l2-server-api");
builder.Services.AddApiApplication(builder.Configuration);
var app = builder.Build();
app.UseServerExceptionHandling();
app.UseRateLimiter();
app.MapServerApi();
app.Run();
