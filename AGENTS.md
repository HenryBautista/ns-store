# AGENTS.md

## What this is

The API half of a rewrite of NS_Store, a store / POS / inventory system for a PC and laptop parts
shop in Cochabamba, Bolivia. ASP.NET Core 10 minimal APIs on PostgreSQL 17, EF Core 10 + Npgsql.
The SPA that consumes it is [`ns-store-ui`](https://github.com/HenryBautista/ns-store-ui), usually
cloned as a sibling directory.

## Map

```
src/NsStore.Domain/          entities, enums, ErrorCodes, AuditableEntity. Depends on nothing
src/NsStore.Application/     Features/<Name>/{XService,XDtos,validators}. Ports in Common/Interfaces
src/NsStore.Infrastructure/  AppDbContext, EF configurations, migrations, JWT/PBKDF2, StockLockService
src/NsStore.Api/             endpoint groups, auth policies, ProblemDetails, rate limiting
tests/NsStore.Domain.Tests/       pure entity invariants
tests/NsStore.Application.Tests/  real services on SQLite in-memory, via TestHarness
tests/NsStore.Architecture.Tests/ the conventions the compiler cannot reach
.agent/domain.md             ES↔EN glossary and business invariants. Read before naming anything
.agent/decisions.md          why the system is like this (D-nn). Read before proposing a redesign
.agent/findings.md           known bugs and traps not being fixed now (DT-nn)
.work/                       scratch space for the current ticket (gitignored)
```

## Hard rules

- **Work is not done until `just check` is green.** It builds, runs every test, and fails if an
  entity changed without a migration.
- **Warnings are errors.** Do not add a blanket `NoWarn` to get a build green — fix it, or suppress
  the one occurrence where you can explain why the analyzer is wrong.
- **The API is locale-agnostic.** It returns codes, enums, ids and numbers, never Spanish display
  copy. A new `errorCode` here is incomplete until `ns-store-ui` has a message for it.
- **A new business term gets a row in `.agent/domain.md` before it is written into the code.**
- **Record a decision that constrains future work in `.agent/decisions.md`**, and a finding you are
  not fixing now in `.agent/findings.md`. Never edit a published decision to change your mind.
- Never push without being asked.

Several rules that used to live here are now enforced and no longer worth reading: the layer
dependency rule, service registration, `DateTime.UtcNow`, the case-sensitive `Enum.TryParse`, and
missing validators all fail `just check` with a message that says what to do.

## Glossary

The six that appear everywhere. Full table, including the Spanish the business actually speaks, in
[`.agent/domain.md`](.agent/domain.md).

- Con factura / Sin factura → `InvoiceType.WithInvoice` / `.WithoutInvoice`
- Pedido / encargo → `Order` · Cotización → `Quote`
- Saldo → `Sale.Balance` · Cobro → `Payment`
- Sucursal → `Branch` (the system **is** multi-branch: one `StockLevel` row per branch)
- Movimiento de inventario → `InventoryMovement`, the stock ledger and source of truth
- Kardex → per-product movement summary

## Before you write code

| If you are going to…                          | Do this first                                        |
|-----------------------------------------------|------------------------------------------------------|
| name a new business concept                   | add its row to `.agent/domain.md`                    |
| change an entity or an `IEntityTypeConfiguration` | `just migration NAME=X`                          |
| add a domain enum                             | `MapEnum<T>` in `Infrastructure/DependencyInjection.cs` **and** a migration |
| add a failure mode                            | `ErrorCodes` constant + a Spanish message in `ns-store-ui` |
| touch sales, stock or serials                 | read `src/NsStore.Application/Features/Sales/README.md` |
| propose changing how something is built       | read `.agent/decisions.md` — it may already say why  |
| see behaviour you cannot explain              | read `.agent/findings.md`                            |
| finish anything                               | `just check` until green                             |
| close a ticket                                | `/close`                                             |

## Architecture

Layers, with the dependency rule `Api → Application → Domain` and
`Infrastructure → Application/Domain`, enforced by `tests/NsStore.Architecture.Tests`.

- **Domain** — entities, enums, `ErrorCodes`, `AuditableEntity` (Id/CreatedBy/CreatedAt/UpdatedAt/
  `DeletedAt` soft delete). Invariants live in entity methods and throw `DomainRuleException`.
- **Application** — one folder per feature under `Features/`, each with `XService.cs` (use cases),
  `XDtos.cs` (request/response records) and validators. Services depend only on ports in
  `Common/Interfaces`: `IAppDbContext`, `ICurrentUser`, `IPasswordHasher`, `ITokenService`,
  `IStockLockService`, plus `TimeProvider`. Plain classes, no MediatR, no repositories — EF
  `IQueryable` projections are the data-access layer.
- **Infrastructure** — `AppDbContext`, `IEntityTypeConfiguration` classes, migrations,
  `AuditInterceptor`, `DatabaseInitializer`, `StockLockService`, JWT/PBKDF2.
- **Api** — endpoint groups mapped in `Program.cs` under `/api/v1`. Endpoints are thin: resolve
  service, call it, wrap in `Results.*`.

### Cross-cutting mechanics

- **Errors.** Services throw `AppException` subclasses (`NotFoundException`, `ConflictException`,
  `ForbiddenException`, `BadRequestException`, `ValidationFailedException`); entities throw
  `DomainRuleException`. `AppExceptionHandler` maps them to RFC 7807 with a stable `errorCode`.
  **Never write a status code or a message in an endpoint** — throw the typed exception. A new
  failure mode needs an `ErrorCodes` constant, and a `DomainStatus` mapping if an entity raises it.
- **Validation.** FluentValidation validators are auto-registered from the Application assembly and
  attached with `.WithValidation<TRequest>()` on the route. The filter is a **silent no-op** when no
  validator exists, which is why a test checks the pairing. Domain invariants stay in the entities.
- **Auth.** JWT bearer access token + rotating refresh token in an httpOnly cookie scoped to
  `/api/v1/auth`. Route policies are `AuthPolicies.Authenticated` / `.AdminOnly`. Row-level
  ownership (a seller edits only their own orders and quotes) is enforced *inside* the service via
  `ICurrentUser.IsAdmin`, not by the policy.
- **Paging.** Every collection takes `?search=&page=&pageSize=` via `PageRequest` and returns
  `PagedResult<T>` through `ToPagedResultAsync`. `pageSize` is clamped to 200 — which is also why
  reports truncate (`DT-03`).
- **Soft delete.** `HasQueryFilter(x => x.DeletedAt == null)` on nearly every entity. Child tables
  (`SaleItem`, `PurchaseItem`) have **no filter of their own**, so a query that reaches one must go
  *through* the filtered parent. The kardex projection in `InventoryService` is the example to copy.
- **Audit.** `AuditInterceptor` stamps CreatedAt/CreatedBy/UpdatedAt on `SaveChanges`. Never set
  them by hand.
- **Naming.** snake_case schema via `UseSnakeCaseNamingConvention`; JSON is camelCase with enums as
  camelCase strings. Query-string enums parse through `QueryEnum.Parse`.
- **Query enums.** Minimal APIs bind enums with a case-sensitive overload, so the API used to reject
  the very spelling it emits. `Enum.TryParse(string, out T)` is banned by the compiler; use
  `QueryEnum.Parse`, which also turns an unknown value into a 400 rather than a 500.

## Tests

- `NsStore.Domain.Tests` — pure entity invariants, no infrastructure.
- `NsStore.Application.Tests` — real services against **SQLite in-memory** via `TestHarness`, which
  wires the real `AppDbContext`, a `FakeCurrentUser`, a `FakeTimeProvider` fixed at 2026-07-24, a
  `NoOpStockLock`, and seeds an admin, settings, a supplier and a client. Add a new service to
  `TestHarness` rather than hand-wiring it per test.
- `NsStore.Architecture.Tests` — layer boundaries, service registration, validator pairing, enum
  mapping. When one fails, the message is the documentation.
- **PostgreSQL-only behaviour is not covered**: row locking and native enums do not exist under
  SQLite, and SQLite stores `decimal` as TEXT, which distorts money check constraints. See the
  traps section of `.agent/findings.md` before writing a test with partial payments.

## Configuration

Nothing secret is committed. Supply via environment variables or user-secrets:
`ConnectionStrings__Default`, `Jwt__SigningKey` (≥32 chars), `Jwt__AccessTokenMinutes` (15),
`Jwt__RefreshTokenDays` (14), `Cors__AllowedOrigins__0`, and `Seed__Admin__Username` /
`Seed__Admin__Password` — the bootstrap admin is created **only while no user exists**.
`appsettings.Development.json` holds throwaway local values.

In Development the OpenAPI doc is at `/openapi/v1.json` and Scalar UI at `/scalar/v1`. `/health` is
anonymous. Migrations and seeding run at startup unless `Database__MigrateOnStartup=false`.

## Commands

Run `just --list`. Do not invoke `dotnet` directly for anything the justfile already covers — CI
runs the same `just check`, so a step that exists only in your shell is a step CI does not have.
