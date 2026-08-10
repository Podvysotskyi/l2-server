# Repository Guidelines

## Scope

This repository owns authoritative Login/Game behavior, player identity, player characters, and Server-owned persistence. Do not put Studio authoring, Admin operations UI/API code, or browser presentation code here.

## Contracts

Consume `L2.Contracts.*` packages for stable cross-service DTOs and immutable release manifests. Do not share EF Core entities, DbContexts, migrations, or domain rules through contracts.

## Commands

```sh
dotnet restore
dotnet build L2.Server.slnx --no-restore
dotnet test L2.Server.slnx --no-build --no-restore
```

## Conventions

Use UTF-8, LF endings, and four-space C# indentation. Server authority and persistence must remain on the backend; clients never determine authoritative outcomes.
