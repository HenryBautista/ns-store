# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Part of a three-repo system

This repo is the API. The SPA is [`ns-store-ui`](https://github.com/HenryBautista/ns-store-ui),
and the documentation and process live in
[`ns-store-docs`](https://github.com/HenryBautista/ns-store-docs), which is also the umbrella
folder the other two are cloned into:

```
ns-store-project/     ← ns-store-docs
├── docs/
├── ns-store/         ← this repo
└── ns-store-ui/
```

**Cloned standalone?** Clone `ns-store-docs` and put this repo inside it. Otherwise the umbrella
`CLAUDE.md`, the delivery checklist and the `/entrega` and `/hallazgo` commands are not loaded, and
the registers below stay invisible.

### Updating the registers is part of finishing

Every session starts with no memory. Without these, it rediscovers the same things, relitigates
decisions already made, and loses the findings it deliberately left out of scope.

| Register | Write to it when | Invariant |
|---|---|---|
| [Findings](https://github.com/HenryBautista/ns-store-docs/blob/main/docs/06-deuda-tecnica.md) | You spot a bug, debt or improvement you are **not** fixing now | `DT-nn` ids are stable — never renumbered or recycled. Nothing is deleted; status changes |
| [Delivery log](https://github.com/HenryBautista/ns-store-docs/blob/main/docs/07-bitacora.md) | You close a delivery | **Append only.** A published entry is never edited, not even to correct it — write a new one saying what changed |
| [Decisions](https://github.com/HenryBautista/ns-store-docs/blob/main/docs/08-decisiones.md) | You make or reverse an architectural decision | A live decision is not edited to change your mind: mark it *reversed by* the new entry |

Plus the [ES↔EN glossary](https://github.com/HenryBautista/ns-store-docs/blob/main/docs/01-vision-general.md):
a new business term gets a row, with its English identifier, **before** it is written into the
code. The domain is thought in Spanish and the code is English; without the bridge fixed up front,
two sessions name the same concept differently.

Run `/entrega` from the umbrella folder to close a delivery: it runs the real definition of done,
records what came out of the work, and writes the log entry pinning both repos' branch and SHA.

> ⚠️ **`DT-10` is open: the schema documentation stopped at the initial migration.** Branches,
> stock transfers, payment receipts and serialized inventory are built and undocumented — and
> `docs/01-vision-general.md` §5 asserts there is *no* multi-branch, which is false. Verify against
> the code before trusting any doc about schema, branches, receipts or serials.

## What this is

The API half of a rewrite of the legacy NS_Store (WPF + SQL Server) store / POS / inventory system:
ASP.NET Core 10 minimal APIs on PostgreSQL 17, EF Core 10 + Npgsql. The SPA that consumes it is
`ns-store-ui`.

Language convention: **all code, identifiers, schema and API contracts are English.** The API is
locale-agnostic — it returns codes, enums, ids and numbers, never Spanish display copy. The SPA maps
`errorCode` values to Spanish, so **a new `errorCode` here is incomplete until `ns-store-ui` has a
message for it** (`src/shared/domain/labels.ts`).

## Commands

```bash
docker compose up -d db                       # Postgres only; appsettings.Development.json points at it
dotnet run --project src/NsStore.Api          # API on :5xxx (launchSettings) / :8080 under Compose
docker compose up --build                     # full stack; needs .env with JWT_SIGNING_KEY set

dotnet build
dotnet test
dotnet test tests/NsStore.Application.Tests                       # one project
dotnet test --filter "FullyQualifiedName~SaleServiceTests"        # one class
dotnet test --filter "FullyQualifiedName~Cash_sale_decrements"    # one test

dotnet ef migrations add <Name> --project src/NsStore.Infrastructure --startup-project src/NsStore.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/NsStore.Infrastructure --startup-project src/NsStore.Api
dotnet ef migrations has-pending-model-changes --project src/NsStore.Infrastructure --startup-project src/NsStore.Api
```

CI (`.github/workflows/ci.yml`) runs restore → build → test → `has-pending-model-changes`. **Any
entity or EF-configuration change without a matching migration fails CI.**

In Development the OpenAPI doc is at `/openapi/v1.json` and Scalar UI at `/scalar/v1`. `/health` is
anonymous. Migrations + seeding run at startup unless `Database__MigrateOnStartup=false`.

## Architecture

Layers with a strict dependency rule `Api → Application → Domain`, `Infrastructure → Application/Domain`:

- **NsStore.Domain** — entities, enums, `ErrorCodes`, `AuditableEntity` (Id/CreatedBy/CreatedAt/UpdatedAt/`DeletedAt` soft delete). Invariants live in entity methods and throw `DomainRuleException` (e.g. `StockLevel.Apply` throws `INSUFFICIENT_STOCK`). No external dependencies.
- **NsStore.Application** — one folder per feature under `Features/`, each holding `XService.cs` (use cases), `XDtos.cs` (request/response records) and validators. Services depend only on ports in `Common/Interfaces`: `IAppDbContext`, `ICurrentUser`, `IPasswordHasher`, `ITokenService`, `IStockLockService`, plus `TimeProvider`. **Services are plain classes registered in `Application/DependencyInjection.cs` — no MediatR, no repositories; EF `IQueryable` projections are the data-access layer.**
- **NsStore.Infrastructure** — `AppDbContext` (implements `IAppDbContext`), `IEntityTypeConfiguration` classes, migrations, `AuditInterceptor`, `DatabaseInitializer`, `StockLockService`, JWT/PBKDF2.
- **NsStore.Api** — endpoint groups mapped in `Program.cs` under `/api/v1`, auth policies, ProblemDetails handler, rate limiting. Endpoints are thin: resolve service, call it, wrap in `Results.*`.

### Cross-cutting mechanics worth knowing before editing

- **Errors**: services throw `AppException` subclasses (`NotFoundException`, `ConflictException`, `ForbiddenException`, `BadRequestException`, `ValidationFailedException`) or entities throw `DomainRuleException`. `AppExceptionHandler` maps them to RFC 7807 with a stable `errorCode` extension. **Never write status codes or messages in an endpoint — throw the typed exception.** New failure modes need an `ErrorCodes` constant and a mapping in `AppExceptionHandler.DomainStatus`.
- **Validation**: FluentValidation validators are auto-registered from the Application assembly; attach with `.WithValidation<TRequest>()` on the route (see `ValidationFilter`). Domain invariants stay in the entities.
- **Auth**: JWT bearer access token + rotating refresh token in an httpOnly cookie scoped to `/api/v1/auth` (`AuthCookies`). Route-level policies are `AuthPolicies.Authenticated` / `AuthPolicies.AdminOnly`. Row-level ownership (orders/quotes: seller edits own, admin edits any) is enforced *inside* the service via `ICurrentUser.IsAdmin` — not by the policy.
- **Paging**: every collection takes `?search=&page=&pageSize=` via `PageRequest` and returns `PagedResult<T>` through `ToPagedResultAsync`. `pageSize` is clamped to 200.
- **Soft delete**: `HasQueryFilter(x => x.DeletedAt == null)` on nearly every entity. Queries that reach a child table (e.g. `SaleItem`, which has no filter of its own) must go *through* the filtered parent — see the kardex projection in `InventoryService`.
- **Audit**: `AuditInterceptor` stamps CreatedAt/CreatedBy/UpdatedAt on `SaveChanges`; don't set them by hand.
- **Time**: inject `TimeProvider`, never `DateTime.UtcNow` — tests substitute a fake clock.
- **Naming**: snake_case DB schema via `UseSnakeCaseNamingConvention`; domain enums are native PostgreSQL enum types registered in `Infrastructure/DependencyInjection.cs` — **adding an enum requires mapping it there plus a migration.** JSON is camelCase with enums as camelCase strings.

### Inventory and sales invariants

These are the parts most easily broken:

- Stock changes only through purchases, sales and admin adjustments. Every change writes an `InventoryMovement` (the ledger, source of truth) **and** updates the per-product `StockLevel` cache, which is never deleted and may sit at 0. Never mutate `StockLevel.Quantity` directly — call `Apply`, which enforces non-negative stock and bumps the `Version` concurrency token.
- Sales are one unit of work: `IAppDbContext.ExecuteInTransactionAsync` (safe to nest; retries the whole action under the Npgsql execution strategy) wrapping `IStockLockService.LockAsync` (`SELECT … FOR UPDATE` on `stock_levels`, ids locked in sorted order to avoid deadlocks) plus the optimistic `Version` check. Oversell surfaces as `409 INSUFFICIENT_STOCK`.
- **Dual price**: each product stores a price with and without invoice; the sale's `InvoiceType` selects which price every line uses.
- **Price suggestion**: `withoutInvoice = lastPurchaseCost × (1 + margin)`, `withInvoice = withoutInvoice × (1 + vat)`. Margin and VAT come from `app_settings` (seeded at legacy 30% / 16%) — never hardcode them.
- Credit sales keep a balance; each installment is a `Payment` row and the sale flips to `paid` at zero balance.

## Tests

- `NsStore.Domain.Tests` — pure entity invariants, no infrastructure.
- `NsStore.Application.Tests` — real services against **SQLite in-memory** via `TestHarness`, which wires the real `AppDbContext` (snake_case, `AuditInterceptor`), a `FakeCurrentUser`, a `FakeTimeProvider` fixed at 2026-07-24, a `NoOpStockLock`, and seeds one admin, settings, a supplier and a client. New application services should be added to `TestHarness` rather than hand-wired per test.
- Postgres-only behaviour (row locking, native enums) is **not** covered by these tests. Integration tests with Testcontainers are not built yet.

## Configuration

Nothing secret is committed. Supply via environment variables or user-secrets: `ConnectionStrings__Default`,
`Jwt__SigningKey` (≥32 chars), `Jwt__AccessTokenMinutes` (15), `Jwt__RefreshTokenDays` (14),
`Cors__AllowedOrigins__0`, and `Seed__Admin__Username` / `Seed__Admin__Password` — the bootstrap admin
is created **only while no user exists**. `appsettings.Development.json` holds throwaway local values.
