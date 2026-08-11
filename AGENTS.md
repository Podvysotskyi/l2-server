# Repository Guidelines

## Scope

This repository owns authoritative Login and Game Server behavior, player identity, player characters, Server-owned content, and its PostgreSQL persistence and migrations. Studio authoring, Admin UI/API operations, browser presentation, and external-service persistence remain outside this repository.

The Server owns gameplay authority. External producers use Server-boundary contracts and map their payloads into Server-owned models. Do not depend on shared entities or import Studio/Admin EF Core models, `DbContext` types, migrations, or domain rules. Admin reads Server-owned data only through narrow internal read APIs or an Admin-owned read model.

## Commands

Run development, every check, and every build through Docker from the repository root:

```sh
docker build --target build --tag l2-server-build .
docker build --target validate --tag l2-server-validate .
docker run --rm --volume "$PWD:/workspace" --workdir /workspace docker:29-cli compose config
docker compose build
```

Do not run development, checks, builds, tests, publishing, or EF Core commands with host-installed .NET tooling.

Start the repository-owned development stack with:

```sh
docker compose up --build postgres redis api-server game-server
```

`api-server` and `game-server` apply the Server migration stream at startup unless `Persistence__RunMigrations=false`. `/health/live` is process liveness; `/health/ready` also verifies that no Server migrations are pending.

## Architecture

- `L2.Server.Api` owns controllers, action-filter validation, authentication, and HTTP composition. Keep controllers thin.
- `L2.Server.Game` owns runtime-host and WebSocket composition. Gameplay protocol handling must delegate authoritative decisions to Server services.
- `L2.Server.Configurations` owns dependency registration, persistence wiring, migration hosting, service identity, and process-level health endpoints.
- `L2.Server.Contracts` groups Server-boundary DTOs by type under `Models`, `Requests`, and `Responses`.
- `L2.Server.Context` owns EF Core entities, identifiers, and the single `L2ServerDbContext`. Entity classes own scalar schema metadata through data annotations; Fluent configuration is limited to relationships, indexes, composite keys, and check constraints.
- `L2.Server.Migrations` owns the only migration stream for `L2ServerDbContext`.
- `L2.Server.Services.Interfaces` owns service abstractions and service-facing models; `L2.Server.Services` owns authoritative orchestration.
- `L2.Server.Repositories.Interfaces` owns persistence abstractions and repository-facing models; `L2.Server.Repositories` owns EF Core implementations and persistence-specific exception translation.
- `L2.Server.Exceptions` owns Server-specific exception types.

Repository, service, API, configuration, and Game test projects must remain database-free unit tests. Keep every public record, interface, and class in its own `.cs` file.

## Configuration and delivery

Environment-specific settings belong in the matching `appsettings.<Environment>.json` file for each host. Standard ASP.NET Core environment variables may override them. The local Compose stack runs Development settings and uses service DNS; published images default to Production.

Server and Compose workflows validate independently on pull requests and `main`. Only pushed `v*` tags publish GHCR images; manual workflow runs never publish.

Never commit connection strings containing real credentials, tokens, original game files, or generated private assets.

## Conventions

Use UTF-8 and LF endings. Preserve established C# formatting, four-space indentation, and nullable-reference-type safety. Clients and external services never determine authoritative outcomes.
