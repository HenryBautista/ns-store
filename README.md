# NS Store

Modern web rewrite of the legacy NS_Store (WPF + SQL Server) store / POS / inventory system.
This repository is the **backend**: an ASP.NET Core (.NET 10) Web API on PostgreSQL. The SPA that
consumes it lives in [`ns-store-ui`](https://github.com/HenryBautista/ns-store-ui).

Domain vocabulary, architectural decisions and known findings live in [`.agent/`](.agent/), next to
the code they describe; [`AGENTS.md`](AGENTS.md) is the entry point. The analysis of the legacy WPF
system is frozen in the archived
[`ns-store-docs`](https://github.com/HenryBautista/ns-store-docs) repository — it is a historical
specification and does not describe how anything is built today.

> Convention: **all code, identifiers, schema and API contracts in English**; only end-user-facing
> text is Spanish, and it lives in the frontend i18n files. The API is locale-agnostic — it returns
> codes, enums, ids and numbers, never display copy.

## Stack

| Layer | Choice |
|---|---|
| API | ASP.NET Core 10, minimal APIs, OpenAPI + Scalar |
| Persistence | EF Core 10 + Npgsql, code-first migrations, snake_case schema, native PostgreSQL enums |
| Auth | JWT access token + rotating refresh token (httpOnly cookie), PBKDF2 password hashing |
| Validation | FluentValidation at the API boundary; domain invariants in the entities |
| Errors | RFC 7807 ProblemDetails with a stable `errorCode` |
| Logging | Serilog (structured) |
| Tests | xUnit; SQLite in-memory for application-service tests |

## Solution layout

```
src/
  NsStore.Domain/          entities, enums, domain rules, error codes — no external dependencies
  NsStore.Application/     use-case services, DTOs, validators, ports (IAppDbContext, ITokenService…)
  NsStore.Infrastructure/  EF Core DbContext, configurations, migrations, JWT, hashing, seeding
  NsStore.Api/             endpoints, auth, ProblemDetails, rate limiting, DI, configuration
tests/
  NsStore.Domain.Tests/
  NsStore.Application.Tests/
```

Dependency rule: `Api → Application → Domain`, `Infrastructure → Application/Domain`.

## Commands

Every task in this repo goes through [`just`](https://github.com/casey/just), a small task runner.
`Justfile` at the root is the list of tasks — it is named after the tool, the way `Makefile` is.

```bash
winget install --id Casey.Just -e   # once per machine; also in brew, apt, cargo, scoop

just          # same as `just check`
just check    # build + every test + the pending-migration guard. ~16s
just --list   # every task, with what it does
```

**`just check` is the definition of done.** CI runs that exact command, so a step that only exists
in your shell is a step CI does not have — put it in the `Justfile` instead.

## Running locally

### 1. Docker Compose (database + API)

```bash
cp .env.example .env          # then fill JWT_SIGNING_KEY and SEED_ADMIN_PASSWORD
just up
```

The API listens on `http://localhost:8080`; `/health` reports readiness.

### 2. API from the SDK, database in Docker

```bash
just db     # Postgres only
just run    # the API on the launchSettings port
```

`appsettings.Development.json` points at the Compose database. In Development the OpenAPI document
is served at `/openapi/v1.json` with the Scalar UI at `/scalar/v1`.

### Configuration

Nothing secret is committed. Supply these through environment variables (or user-secrets):

| Setting | Environment variable | Notes |
|---|---|---|
| Connection string | `ConnectionStrings__Default` | least-privilege database user, not a superuser |
| JWT signing key | `Jwt__SigningKey` | ≥ 32 characters |
| Access token lifetime | `Jwt__AccessTokenMinutes` | default 15 |
| Refresh token lifetime | `Jwt__RefreshTokenDays` | default 14 |
| Allowed SPA origins | `Cors__AllowedOrigins__0` | omit when the SPA is served same-site |
| Bootstrap admin | `Seed__Admin__Username`, `Seed__Admin__Password` | used **only** while no user exists |

Migrations are applied on startup; set `Database__MigrateOnStartup=false` to apply them as a
separate deployment step instead.

### Migrations

```bash
just migration AddSomething   # create it
just migrate-db               # apply to the local database
just schema                   # fail if the model changed without a migration
```

`dotnet-ef` is a pinned local tool (`.config/dotnet-tools.json`), so the version cannot drift
between machines and CI. `just check` runs `just schema`, which is why an entity or
`IEntityTypeConfiguration` change without a matching migration fails before it reaches CI.

### Tests

```bash
just test
```

Three projects: entity invariants, application services on SQLite in-memory, and
`NsStore.Architecture.Tests` — the conventions the compiler cannot reach (the layer dependency
rule, service registration, enum mapping, validator pairing). When one fails, its message is the
documentation.

## API surface

Base path `/api/v1`. Every endpoint requires a bearer token except `/auth/login` and `/auth/refresh`.
Collections accept `?search=&page=&pageSize=` and return `{ items, page, pageSize, total }`.

| Area | Endpoints |
|---|---|
| Auth | `POST /auth/login`, `POST /auth/refresh`, `POST /auth/logout`, `GET /auth/me` |
| Users (admin) | `GET/POST /users`, `GET/PUT /users/{id}`, `PATCH /users/{id}/status`, `PATCH /users/{id}/role` |
| Catalogs | CRUD on `/trademarks`, `/categories`, `/warranty-terms`, `/suppliers` |
| Products | CRUD on `/products`, `GET /products/{id}/price-suggestion`, `PUT /products/{id}/prices` (admin), `GET /products/{id}/movements` |
| Inventory | `GET /stock`, `POST /stock/adjustments` (admin), `GET /kardex` |
| Clients | CRUD on `/clients`, `GET /clients/{id}/sales` |
| Purchases | `GET/POST /purchases`, `GET /purchases/{id}` |
| Sales | `GET/POST /sales`, `GET /sales/{id}`, `GET /sales/{id}/items`, `POST /sales/{id}/payments`, `GET /sales/debts` |
| Orders / Quotes | CRUD on `/orders` and `/quotes` (delete is admin-only) |
| Settings | `GET /settings`, `PUT /settings` (admin) |
| Reports | `/reports/dashboard`, `/sales`, `/purchases`, `/stock`, `/debts`, `/price-list`, `/sale-invoice/{saleId}`, `/order/{id}`, `/quote/{id}` |

### Authorization

| Area | seller | admin |
|---|---|---|
| Catalogs, products, clients, stock, purchases, sales, payments, kardex, reports | ✅ | ✅ |
| Orders/quotes — create | ✅ | ✅ |
| Orders/quotes — edit | own only | any |
| Orders/quotes — delete | ❌ | ✅ |
| Users, settings, price changes, manual stock adjustments | ❌ | ✅ |

### Error codes

`VALIDATION_ERROR`, `INVALID_CREDENTIALS`, `UNAUTHORIZED`, `FORBIDDEN`, `NOT_FOUND`, `CONFLICT`,
`INSUFFICIENT_STOCK`, `CONCURRENCY_CONFLICT`, `DUPLICATE_USERNAME`, `DUPLICATE_NAME`,
`PAYMENT_EXCEEDS_BALANCE`, `ADVANCE_EXCEEDS_PRICE`, `PRICE_NOT_SET`, `INVALID_REFRESH_TOKEN`,
`INTERNAL_ERROR`. The SPA maps each code to its Spanish message.

## Business rules worth knowing

- **Dual price**: each product carries a price with and without invoice; the invoice type chosen for
  a sale selects the price for every line of that sale.
- **Price suggestion**: `withoutInvoice = lastPurchaseCost × (1 + margin)`, `withInvoice =
  withoutInvoice × (1 + vat)`. Margin and VAT come from `app_settings` (seeded at the legacy values
  30% and 16%) — **confirm the current VAT rate with the business**.
- **Stock** moves only through purchases, sales and explicit manual adjustments. Every change writes
  an `inventory_movements` row (the ledger and kardex source of truth) and updates the per-product
  `stock_levels` cache, which is never deleted and may sit at 0.
- **Sales are atomic**: pricing, stock validation, decrement, ledger entries and the initial payment
  all happen in one transaction, with the stock rows locked (`SELECT … FOR UPDATE`) plus an
  optimistic version column. Overselling returns `409 INSUFFICIENT_STOCK`.
- **Credit sales** keep a balance; each installment is a `payments` row, and the sale flips to
  `paid` when the balance reaches zero.
- **Soft delete** everywhere (`deleted_at`), with audit columns on every table.

## Not built yet

- Server-side PDF rendering: report endpoints return structured data for printable views.
- API integration tests with Testcontainers (Phase 9 of the roadmap).
