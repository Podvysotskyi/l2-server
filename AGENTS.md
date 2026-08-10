# Repository Guidelines

## Scope

This repository owns authoritative Login/Game behavior, player identity, player characters, and Server-owned persistence. Do not put Studio authoring, Admin operations UI/API code, or browser presentation code here.

## Contracts

Define external input contracts at the Server boundary and map producer payloads into Server-owned models. Do not depend on a shared contracts repository or share EF Core entities, DbContexts, migrations, or domain rules across services.

## Commands

```sh
dotnet restore
dotnet build L2.Server.slnx --no-restore
dotnet test L2.Server.slnx --no-build --no-restore
```

## Conventions

Use UTF-8, LF endings, and four-space C# indentation. Server authority and persistence must remain on the backend; clients never determine authoritative outcomes.
