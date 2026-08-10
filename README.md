# L2 Server

Authoritative L2 backend for player identity, character management, gameplay-session authority, and Server-owned game content. It provides an HTTP API and a separate Game Server runtime, both backed by the same PostgreSQL database.

## Architecture

The .NET solution is split into focused projects:

- `L2.Server.Api` — HTTP host, controllers, request filters, authentication, and HTTP composition
- `L2.Server.Game` — gameplay runtime host, WebSocket endpoints, and protocol-session composition
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

The Server owns gameplay authority and persistence. Studio authoring models, Admin UI/API code, browser code, and external-service entities do not belong here. Producers map their inputs into Server-owned contracts; no other service imports Server EF entities, `DbContext` types, migrations, or domain rules.

`L2.Server.Api` issues opaque, hashed game-session tokens and authorizes character management over HTTP. It exchanges an authenticated cookie for a single-use game ticket. `L2.Server.Game` validates the ticket-derived game session and opens protocol-v2 gameplay WebSockets only after character selection.

## Prerequisites

- Docker Engine with Docker Compose

## Development

Start the local Server stack:

```sh
docker compose up --build postgres redis api-server game-server
```

Compose starts PostgreSQL, Redis, the Server API, and the Game Server. The API is available at <http://localhost:5001>; the Game Server health endpoints are at <http://localhost:5002>. PostgreSQL and Redis are bound to `localhost:5432` and `localhost:6379`.

Both hosts use the `l2-server` PostgreSQL database and its `public` schema. Compose uses Production app settings, which connect to the `postgres` service. Development app settings use `localhost`. The checked-in development stack uses database `l2-server`, user `l2`, and password `secret`; override settings through standard ASP.NET Core configuration when needed.

The API and Game Server apply pending Server migrations at startup by default. Set `Persistence__RunMigrations=false` only when migrations are applied separately. Their `/health/live` endpoints report process liveness; `/health/ready` also requires the migration state to be current.

If `postgres-data` was initialized with different credentials, recreate that named volume before starting the checked-in stack.

GitHub workflows validate the Server and Compose model independently. Pull requests and `main` pushes validate only. Pushing a `v*` tag validates and publishes `ghcr.io/podvysotskyi/l2-server-api` and `ghcr.io/podvysotskyi/l2-game-server` with the Git tag and `latest` tags.

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

When this repository is checked out through `l2-infra`, use `$develop-l2-server-api` for identity, character, session, persistence, migration, and HTTP API work. Use `$develop-l2-game-server` for gameplay WebSocket, connection, protocol, and runtime work. Use both for changes to the ticket exchange, game-session token, selected-character state, or another contract shared by the two hosts.

## Security

Do not commit production connection strings, credentials, tokens, original game files, or generated private assets.
