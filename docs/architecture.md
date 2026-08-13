# L2 Server architecture

L2 Server owns authoritative account, session, character, world-session, and
gameplay state. Browser clients and external services may request operations,
but only Server services decide their outcomes.

## Hosts and sessions

`L2.Server.Api` is the shared account and discovery host. It authenticates
accounts globally, lists enabled game versions and worlds, aggregates world
readiness, and issues single-use tickets bound to a version and world.

`L2.Server.Game.C1`, `L2.Server.Game.C4`, and
`L2.Server.Game.Interlude` are thin version-fixed hosts over
`L2.Server.Game.Runtime`. Deployment configuration supplies the world key.
Each Game host exchanges only compatible tickets, persists a revocable Game
session, exposes authenticated character management over HTTP, and opens the
gameplay WebSocket only after character selection. Browser access tokens remain
in memory and are never placed in URLs or persisted by the client.

## Persistence and authority

All Server-owned records use the single `L2ServerDbContext` and the migration
stream in `L2.Server.Migrations`. Server hosts may apply that stream; Admin and
Studio never do. Accounts are global, while tickets, Game sessions, characters,
names, and slots are isolated by game version and world where applicable.

Studio owns authored content, asset catalogs, generated artifacts, and release
publication in a separate database. Server does not query Studio persistence or
generated files directly. Published inputs cross an explicit Server-owned
contract; the temporary in-memory character-creation provider remains isolated
until that publication boundary is implemented.

Admin currently reads selected Server tables through independently owned,
read-only queries. It does not import Server implementation types or receive
migration authority.

## Contract ownership

Public HTTP and WebSocket DTOs belong to `L2.Server.Contracts`. Service and
repository models remain behind their corresponding interfaces. Consumers keep
their own structurally compatible browser or read models instead of sharing EF
entities, domain models, or migrations.
