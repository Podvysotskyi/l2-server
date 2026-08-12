# L2 Server

Authoritative L2 backend for player identity, character management, and gameplay-session authority. It provides one shared Server API and separate Chronicle 1, Chronicle 4, and Interlude Game hosts backed by the same PostgreSQL database.

## Architecture

The .NET solution is split into focused projects:

- `L2.Server.Api` — HTTP host, controllers, request filters, authentication, and HTTP composition
- `L2.Server.Game.Runtime` — shared Game HTTP, character, WebSocket, and protocol-session composition
- `L2.Server.Game.C1`, `L2.Server.Game.C4`, and `L2.Server.Game.Interlude` — thin version-fixed Game hosts
- `L2.Server.Configurations` — dependency registration, persistence, service identity, migration hosting, health endpoints, and process-level configuration
- `L2.Server.Contracts` — Server-owned models, requests, responses, and protocol contracts
- `L2.Server.Context` — EF Core entity definitions, identifiers, and the `L2ServerDbContext`
- `L2.Server.Migrations` — the authoritative EF Core migration stream
- `L2.Server.Services.Interfaces` — service abstractions and service-facing models
- `L2.Server.Services` — authoritative application orchestration
- `L2.Server.Repositories.Interfaces` — persistence abstractions and repository-facing models
- `L2.Server.Repositories` — EF Core persistence implementations and database exception classification
- `L2.Server.Exceptions` — Server-specific exception types
- `tests/L2.Server.*.Tests` — database-free unit tests grouped by owning layer

The Server owns gameplay authority and player persistence. Studio owns game-content entities and authoring data, including NPCs, skills, player classes, appearance options, and asset catalogs. Character creation temporarily uses a minimal in-memory content provider and stores the selected IDs and an `IsMage` snapshot with the character. The provider is intentionally isolated so it can be replaced when Studio publishes character-creation content to Redis. Studio authoring models, Admin UI/API code, browser code, and external-service entities do not belong here. Producers map their inputs into Server-owned contracts; no other service imports Server EF entities, `DbContext` types, migrations, or domain rules.

`L2.Server.Api` owns global account authentication, version/world discovery, readiness aggregation, and single-use ticket issuance. Each version Game host exchanges its own world-bound tickets, authorizes character management over HTTP, and opens protocol-v2 gameplay WebSockets only after character selection.

The Server API exposes enabled versions at `GET /api/game-versions` and a
health-aggregated world list at `GET /api/game-versions/{version}/servers`.
Login is account-global. Version and world keys are retained through the ticket,
Game session, character operations, and gameplay socket.

## Prerequisites

- Docker Engine with Docker Compose

## Development

Start the local Server stack:

```sh
docker compose up --build postgres redis api-server game-interlude-default game-c1-default game-c4-default
```

Compose starts PostgreSQL, Redis, the Server API, and all three default worlds. The API is available at <http://localhost:5001>; Interlude, C1, and C4 are available on ports 5002, 5003, and 5004 respectively.

Both hosts use the `l2-server` PostgreSQL database and its `public` schema. Compose sets `DOTNET_ENVIRONMENT=Development`, whose settings connect to the `postgres` service. The checked-in development stack uses database `l2-server`, user `l2`, and password `secret`; override settings through standard ASP.NET Core configuration when needed.

The API and Game Server apply pending Server migrations at startup by default. Set `Persistence__RunMigrations=false` only when migrations are applied separately. Their `/health/live` endpoints report process liveness; `/health/ready` also requires the migration state to be current.

If `postgres-data` was initialized with different credentials, recreate that named volume before starting the checked-in stack.

The current migration baseline was rewritten when game-content persistence moved to Studio. Recreate existing Server development database volumes before starting this version.

GitHub workflows validate the Server and Compose model independently. Pushing a `v*` tag publishes the Server API plus separate `l2-game-server-c1`, `l2-game-server-c4`, and `l2-game-server-interlude` images.

Game-version discovery advertises `/versions/{version}/current.json` on the configured Asset Server. Studio owns that live release pointer; the Server does not read Studio persistence or generated assets directly.

## Checks

Run builds, tests, and Compose validation inside Docker from the repository root:

```sh
docker build --target build --tag l2-server-build .
docker build --target validate --tag l2-server-validate .
docker run --rm --volume "$PWD:/workspace" --workdir /workspace docker:29-cli compose config
docker compose build
```

The `validate` target restores dependencies, builds the Release solution, and runs every Server test project. Do not use host-installed .NET tooling for development, builds, tests, publishing, or migration operations.

## Codex skills

When this repository is checked out through `l2-infra`, use `$develop-l2-server` for Login API, Game Server, persistence, migration, protocol, and cross-host session work. The skill preserves the shared authority and contract boundaries between both hosts.

## Security

Do not commit production connection strings, credentials, tokens, original game files, or generated private assets.
