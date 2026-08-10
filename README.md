# L2 Server

The authoritative L2 backend. This repository will own the Login Server, Game Server, player identity, player characters, and Server-owned persistence/migrations.

## Layout

```text
src/    Server applications and owned libraries
tests/  Server-focused unit and integration tests
```

## Commands

```sh
dotnet restore
dotnet build L2.Server.slnx --no-restore
dotnet test L2.Server.slnx --no-build --no-restore
```

## Boundaries

Server owns gameplay authority and persistence. It consumes immutable content-release contracts from `l2-contracts`; it must not reference Studio authoring EF Core models. Admin reads Server-owned information through narrow internal read APIs or an Admin-owned read model, never by importing Server `DbContext` classes.
