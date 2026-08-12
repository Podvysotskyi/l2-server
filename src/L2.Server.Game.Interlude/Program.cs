using L2.Server.Game;
using L2.Server.Context.Identifiers;

var builder = WebApplication.CreateBuilder(args)
    .AddGameRuntime("l2-server-game-interlude", GameVersionIdentifiers.Interlude);
var app = builder.Build();
app.MapGameRuntime();
app.Run();
