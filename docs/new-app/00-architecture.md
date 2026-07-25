# 00 — Architecture

## 1. High-level topology

```
┌──────────────────────────┐        HTTPS / JSON        ┌───────────────────────────┐
│   React SPA (TypeScript)  │  ───────────────────────►  │   ASP.NET Core Web API     │
│   Vite · Node 24 build    │  ◄───────────────────────  │   .NET 10                  │
│   react-i18next (es-BO)   │      JWT (access) +        │   EF Core 10               │
└──────────────────────────┘   refresh cookie (httpOnly) └────────────┬──────────────┘
                                                                       │ Npgsql / EF Core
                                                                       ▼
                                                            ┌───────────────────────┐
                                                            │      PostgreSQL        │
                                                            └───────────────────────┘
```

- Frontend and API are separate deployables. In production, prefer serving them under the
  **same site/domain** (reverse proxy) so the refresh-token cookie is first-party and
  `SameSite=Strict/Lax` works without cross-site relaxation.
- All traffic over **HTTPS**. HSTS enabled.

## 2. Backend solution structure (.NET 10)

Clean/layered architecture. One solution, four projects + tests:

```
NsStore.sln
├── src/
│   ├── NsStore.Domain/          # Entities, value objects, enums, domain rules. No external deps.
│   ├── NsStore.Application/     # Use cases (CQRS-style handlers), DTOs, validators, interfaces (ports).
│   ├── NsStore.Infrastructure/  # EF Core DbContext, repositories, migrations, JWT, hashing, settings.
│   └── NsStore.Api/             # ASP.NET Core Web API: controllers/endpoints, middleware, DI, config.
└── tests/
    ├── NsStore.Domain.Tests/
    ├── NsStore.Application.Tests/
    └── NsStore.Api.IntegrationTests/   # WebApplicationFactory + Testcontainers (PostgreSQL).
```

**Dependency rule:** `Api → Application → Domain`; `Infrastructure → Application/Domain`;
`Domain` depends on nothing. Application defines interfaces; Infrastructure implements them.

**Recommended libraries**

| Concern | Library |
|---|---|
| ORM / migrations | EF Core 10 + Npgsql |
| Validation | FluentValidation |
| Mapping | Mapperly (source-gen) or manual mapping |
| Auth | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| Password hashing | ASP.NET Core `PasswordHasher<T>` (PBKDF2) or **Argon2** (Isopoh/Konscious) |
| Logging | Serilog (structured, to console/file/OTel) |
| API docs | OpenAPI (built-in) + Swagger UI in non-prod |
| Testing | xUnit, FluentAssertions, Testcontainers |

> CQRS is optional but recommended (MediatR-style handlers or plain application services).
> Keep it lightweight; do not over-engineer for a system of this size.

## 3. Cross-cutting concerns

### 3.1 Authentication & authorization
- **JWT access token** (~15 min): returned in the login response body, held **in memory** by the SPA.
- **Refresh token** (long-lived, e.g. 7–30 days): random opaque token, stored **hashed** in
  `refresh_tokens`, delivered as an **httpOnly, Secure, SameSite** cookie. Rotated on each refresh
  (old token revoked); reuse detection revokes the token family.
- Endpoints: `POST /auth/login`, `POST /auth/refresh`, `POST /auth/logout`.
- **Authorization:** role-based (`admin`, `seller`) via policies. Ownership checks for orders/quotes
  (a `seller` may edit only records they own; `admin` may edit/delete any). Enforced in the
  Application layer, not the UI.
- Passwords stored only as **hashes**. No plaintext, ever. No password in logs.

### 3.2 Validation
- Request DTOs validated with **FluentValidation** at the API boundary.
- Domain invariants enforced in entities/aggregates (e.g., cannot sell more than available stock;
  advance amount ≤ order price; paid amount ≤ sale total).

### 3.3 Error handling
- Global exception middleware → **RFC 7807 ProblemDetails** responses.
- Distinguish `400` (validation), `401`/`403` (auth), `404`, `409` (conflict, e.g. insufficient
  stock / concurrency), `500`. **No empty catch blocks** (the legacy's cardinal sin).
- Correlation id per request for tracing.

### 3.4 Transactions & concurrency
- Compound operations (create sale, create purchase, register payment) run inside a **single DB
  transaction** in an application-service/use-case.
- **Concurrency control on stock:** re-check available quantity inside the transaction; use row
  locking (`SELECT ... FOR UPDATE` on `stock_levels`) or optimistic concurrency (`xmin`/version)
  to prevent oversell. Return `409` on conflict.

### 3.5 Configuration & secrets
- `appsettings.json` for non-secret config; **environment variables / user-secrets / vault** for
  connection strings and JWT signing keys. **No secrets in the repo** (legacy shipped `sa` +
  password in source — never repeat).
- DB user with **least privilege** (not a superuser).
- Business parameters (VAT rate, default margin) live in **`app_settings`** table, editable by admin,
  not hardcoded.

### 3.6 Logging & observability
- Structured logging (Serilog). Audit key business events (login, sale, purchase, payment,
  user enable/disable) with actor + timestamp.
- Health checks (`/health`) and OpenTelemetry-ready.

### 3.7 Internationalization
- **API is locale-agnostic**: returns codes/enums/IDs, not display strings. Money as numbers,
  dates as ISO-8601 `timestamptz`.
- **All user-facing text lives in the frontend** i18n resource files (Spanish, `es-BO`). This keeps
  the "English code / Spanish UI" rule clean: the backend never emits Spanish copy.
- Error responses carry stable **error codes**; the frontend maps codes → Spanish messages.

## 4. Frontend architecture (summary — details in [03](03-frontend.md))
- React + TypeScript, **Vite**, Node 24.
- Routing (React Router), server state (**TanStack Query**), forms (**react-hook-form + zod**),
  i18n (**react-i18next**, Spanish), a component library (MUI or shadcn/ui + Tailwind).
- Auth: access token in memory, silent refresh via the httpOnly cookie; route guards by role.

## 5. Environments & delivery
- **Local:** Docker Compose (PostgreSQL + API + web) for a one-command dev environment.
- **CI:** build, test (unit + integration with Testcontainers), lint, EF migration check.
- **CD:** containerized API and static web bundle behind a reverse proxy (HTTPS).
- **DB migrations:** EF Core migrations, applied on deploy (or via a migration job).
