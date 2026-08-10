# L2 Server

The authoritative L2 backend. This repository owns the Login Server, Game Server, player identity, player characters, and Server-owned content and persistence migrations.

## Layout

```text
src/L2.LoginServer/       Player authentication and session issuance
src/L2.GameServer/        Game session handoff and authoritative endpoints
src/L2.PlayerIdentity/    Account and credential persistence
src/L2.PlayerCharacters/  Character persistence and state transitions
src/L2.GameContent/       Runtime game-content persistence
src/L2.Shared/            Server-local hosting and observability support
tests/L2.Server.Tests/    Server-focused unit and integration tests
```

## Commands

```sh
dotnet restore
dotnet build L2.Server.slnx --no-restore
dotnet test L2.Server.slnx --no-build --no-restore
```

## Boundaries

Server owns gameplay authority and persistence. It consumes immutable content-release contracts from `l2-contracts`; it must not reference Studio authoring EF Core models. Admin reads Server-owned information through narrow internal read APIs or an Admin-owned read model, never by importing Server `DbContext` classes.
