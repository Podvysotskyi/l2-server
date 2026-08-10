using L2.GameContent;
using L2.GameServer.Sessions;
using L2.PlayerIdentity;
using L2.PlayerCharacters;
using L2.Shared;

var builder = WebApplication.CreateBuilder(args)
    .AddL2Foundation("l2-game-server");
builder.Services.Configure<GameSessionOptions>(builder.Configuration.GetSection(GameSessionOptions.SectionName));
builder.Services.AddGameContentPersistence(builder.Configuration);
builder.Services.AddPlayerIdentityPersistence(builder.Configuration);
builder.Services.AddPlayerCharacterPersistence(builder.Configuration);
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<GameSessionTicketRepository>();
builder.Services.AddHealthChecks()
    .AddGameContentMigrationHealthCheck()
    .AddPlayerIdentityMigrationHealthCheck()
    .AddPlayerCharacterMigrationHealthCheck();

var app = builder.Build();
app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(30) });
app.MapL2Foundation();
app.MapGameSessions();
app.Run();

namespace L2.GameServer { public sealed class GameServerMarker; }
