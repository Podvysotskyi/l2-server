using L2.Server.Game.Sessions;
using L2.Server.Configurations;

var builder = WebApplication.CreateBuilder(args)
    .AddServerHost("l2-server-game");
builder.Services.AddGameApplication(builder.Configuration);
builder.Services.AddSingleton<GameSessionConnectionHandler>();

var app = builder.Build();
app.UseServerExceptionHandling();
app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(30) });
app.MapServerHost();
app.MapGameSessions();
app.Run();
