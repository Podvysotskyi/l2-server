using L2.Server.Game;
using L2.Server.Context.Identifiers;

var builder = WebApplication.CreateBuilder(args)
    .AddGameRuntime("l2-server-game-c1", GameVersionIdentifiers.C1);
var app = builder.Build();
app.MapGameRuntime();
app.Run();
