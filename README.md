# L2 Server

The authoritative L2 backend. This repository owns the Server API, Game Server, player identity, player characters, and Server-owned content and persistence migrations.

## Layout

```text
src/L2.Server.Api/        L2.Server.Api HTTP host, controllers, and filters
src/L2.Server.Game/       L2.Server.Game runtime host
src/L2.Server.Contracts/  Public Requests, Responses, Classes, and protocol contracts
src/L2.Server.Context/    Entity definitions, identifiers, seed data, and the EF Core context
src/L2.Server.Migrations/ EF Core migration stream for the Server context
src/L2.Server.Configurations/ Dependency, persistence, and HTTP composition
src/L2.Server.Services*/  Service interfaces and authoritative orchestration
src/L2.Server.Repositories*/ Repository interfaces and implementations
```

## Commands

```sh
docker build --target build --tag l2-server-build .
```

## Docker Compose

Run the Server API, Game Server, PostgreSQL, and Redis directly from this repository:

```sh
docker compose up --build postgres redis api-server game-server
```

The Server API is available at <http://localhost:5001>, the Game Server is available for runtime health checks at <http://localhost:5002>, PostgreSQL is bound to `localhost:5432`, and Redis is bound to `localhost:6379`.

Both hosts connect to the same `l2-server` database and use PostgreSQL's single `public` schema.

Development settings connect to `localhost`; Production settings connect to the Compose `postgres` service. Both use database `l2-server` with user `l2` and password `secret`; no `.env` file is required.

If `postgres-data` was initialized with different credentials, recreate that named volume before starting the updated stack.

Game-session access tokens are random, opaque values stored as hashes in PostgreSQL. The API issues them and the Game Server validates them for gameplay WebSockets.

The Server API exchanges its authenticated cookie for a single-use game ticket and then an opaque game-session token. It authorizes character management over HTTP; the Game Server opens protocol-v2 gameplay WebSockets only after character selection.

## Boundaries

Server owns gameplay authority and persistence. External inputs use contracts defined at the Server boundary and are mapped into Server-owned models; Server must not reference Studio authoring EF Core models. Admin reads Server-owned information through narrow internal read APIs or an Admin-owned read model, never by importing Server `DbContext` classes.
