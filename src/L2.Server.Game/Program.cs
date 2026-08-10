using L2.Server.Game.Sessions;
using L2.Server.Configurations;

var builder = WebApplication.CreateBuilder(args)
    .AddServerFoundation("l2-server-game");
builder.Services.AddGameApplication(builder.Configuration);

var app = builder.Build();
app.UseServerExceptionHandling();
app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(30) });
app.MapServerFoundation();
app.MapGameSessions();
app.Run();

namespace L2.Server.Game { public sealed class GameServerMarker; }
