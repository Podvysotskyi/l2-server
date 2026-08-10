# Repository Guidelines

## Scope

This repository owns authoritative Login/Game behavior, player identity, player characters, and Server-owned persistence. Do not put Studio authoring, Admin operations UI/API code, or browser presentation code here.

## Contracts

Define external input contracts at the Server boundary and map producer payloads into Server-owned models. Do not depend on a shared contracts repository or share EF Core entities, DbContexts, migrations, or domain rules across services.

## Commands

Run development, validation, tests, and builds through Docker. Do not use host-installed .NET tooling.

```sh
docker build --target build --tag l2-server-build .
```

`L2.Server.Api` and `L2.Server.Game` are thin hosts. Public DTOs belong in Contracts, dependency composition in Configurations, orchestration in Services, persistence abstractions in Repositories.Interfaces, and implementations in Repositories. Keep controllers thin and every public record, interface, and class in its own file.

## Conventions

Use UTF-8, LF endings, and four-space C# indentation. Server authority and persistence must remain on the backend; clients never determine authoritative outcomes.
