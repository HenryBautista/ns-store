# 04 — Implementation roadmap

> Phased delivery. Each phase is independently shippable/testable and builds on the previous one,
> following the dependency order from legacy doc 01 (§4). "DoD" = Definition of Done.

## Global Definition of Done (every phase)
- Code in English; user-facing text in Spanish via i18n.
- Backend: unit tests for domain rules + integration tests (Testcontainers PostgreSQL) for use cases.
- Frontend: component tests for critical flows; E2E for the phase's happy path.
- EF Core migration created and applied; no secrets in repo.
- OpenAPI updated; endpoints authorized per the matrix; ProblemDetails errors.
- CI green (build, test, lint, migration check).

---

## Phase 0 — Foundations (infrastructure & skeleton)
**Goal:** empty but wired end-to-end app.

- Repos/solution: `NsStore.{Domain,Application,Infrastructure,Api}` + `web/` (Vite).
- Docker Compose: PostgreSQL + API + web; one-command local dev.
- EF Core + Npgsql; base `DbContext`; initial migration (empty).
- Serilog, global exception → ProblemDetails, health checks, OpenAPI.
- Frontend shell: router, TanStack Query, i18n (`es-BO`), theme, layout, HTTP client with interceptors.
- CI pipeline.

**DoD:** `/health` green; SPA loads; a trivial ping endpoint works through the auth-less path.

## Phase 1 — Auth & users
**Goal:** secure login and user administration (unblocks everything else — ownership/audit).

- `users`, `refresh_tokens`; password hashing (Argon2/PBKDF2).
- `POST /auth/login|refresh|logout`, `GET /auth/me`; JWT + rotating refresh cookie; reuse detection.
- Role policies (`admin`/`seller`); `RequireAuth`/`RequireRole` on frontend.
- Users CRUD (admin), enable/disable, duplicate-username guard. Seed an initial admin (via migration/CLI, not hardcoded).
- Login page, session restore on load, route guards, admin-only nav hiding.

**DoD:** disabled user can't log in; seller can't reach `/users`; refresh flow works on hard reload.

## Phase 2 — Catalogs & products
**Goal:** master data.

- `trademarks`, `categories`, `warranty_terms`, `suppliers`, `products` (+ soft delete, audit).
- Uniform catalog CRUD endpoints + product CRUD (prices default 0, not editable here).
- Frontend: catalog admin screens + product screen with trademark/category/warranty pickers; product mode reused as a picker later.
- `app_settings` table seeded (`vat_rate`, `default_margin_pct`, `currency`) + admin Settings screen.

**DoD:** can create the full product catalog; deletes are soft; settings editable by admin.

## Phase 3 — Purchases, inventory ledger & stock
**Goal:** stock inflow + the ledger backbone.

- `purchases`, `purchase_items`, `inventory_movements`, `stock_levels`.
- `POST /purchases` transactional (creates lines + movements + increments stock cache).
- `GET /stock`, `/kardex`, `/products/{id}/movements`, manual `POST /stock/adjustments` (admin).
- Frontend: purchase entry (supplier+product cart), stock list, kardex, product detail movements.

**DoD:** a purchase increments stock atomically and appears in kardex/ledger; stock never negative.

## Phase 4 — Pricing
**Goal:** sale-price management with suggestion.

- `GET /products/{id}/price-suggestion` (last cost + margin + VAT from settings); `PUT /products/{id}/prices`.
- Frontend pricing screen: suggestion vs manual; price list view + printable/report.

**DoD:** suggestion matches formula (cost×(1+margin), ×(1+vat)); prices persist; parameters come from settings, not constants.

## Phase 5 — Clients
**Goal:** customer master (individual/company).

- `clients` CRUD with `type` discriminator; computed `fullName`; search.
- Frontend client screens + reusable client picker for POS.

**DoD:** both client types round-trip correctly; search by name/last names.

## Phase 6 — Sales (POS), debts & payments — core value
**Goal:** the central flow.

- `sales`, `sale_items`, `payments`.
- `POST /sales` transactional: price by invoice type, validate + lock stock, decrement, movements,
  initial payment, warranty-note data. `409 INSUFFICIENT_STOCK`/`CONCURRENCY_CONFLICT`.
- `GET /sales`, `/sales/{id}`, `/sales/debts`, `/clients/{id}/sales`; `POST /sales/{id}/payments`.
- Frontend: POS cart with live stock checks, invoice toggle, contado/crédito, amount paid/balance;
  sales list + detail + payment history + register installment; debts view; sales-by-client.

**DoD:** end-to-end sale decrements stock atomically; oversell/concurrency rejected; credit sale
tracked; installments update balance/status; sale appears in kardex.

## Phase 7 — Orders & quotes
**Goal:** encargos and proformas with ownership.

- `orders`, `quotes` CRUD; owner set on create; seller edits own only; delete admin-only; advance ≤ price.
- Frontend screens with search (client/product/date) and owner-aware buttons.

**DoD:** ownership/permission rules enforced server-side (not just UI); advance-≤-price enforced by constraint.

## Phase 8 — Reports & dashboard
**Goal:** printable outputs and the home overview.

- Report data/PDF endpoints: warranty note (standard + credit), sales, purchases, stock, debts,
  price list, order, quote. (PDF via QuestPDF server-side or printable client views.)
- Dashboard tiles (sales by range, purchases, stock, debts, price list, orders, quotes) + report buttons.

**DoD:** every legacy report has an equivalent; dashboard mirrors `Main_View` tiles.

## Phase 9 — Hardening & launch
- Security review (authz matrix, token handling, rate limiting on `/auth/login`, HSTS/HTTPS).
- Performance (indexes, N+1 checks), pagination everywhere, audit logging of key events.
- Accessibility & responsive pass; error-code → Spanish message coverage.
- Backup/restore runbook; deployment (containers + reverse proxy); initial catalog data load.

**DoD:** parity checklist (legacy doc 05 §4) fully green; security review passed; production deploy.

---

## Suggested build order rationale
`Auth → Catalogs/Products → Purchases/Stock → Pricing → Clients → Sales/Payments → Orders/Quotes →
Reports/Dashboard` matches the data-dependency graph: you can't price without a purchase cost, can't
sell without stock and a client, and reports aggregate everything.

## Open business questions to resolve before/within relevant phases
Carried over from legacy doc 05 §5 — confirm with the business:
1. **VAT rate** (legacy 16%) — set correct value in `app_settings` (Phase 2/4).
2. **Default margin** (legacy 30%) — fixed vs per category/product (Phase 4).
3. Keep the **with/without invoice** dual-price model as-is? (Phase 4/6).
4. **Currency** (assumed BOB) and any multi-currency need (Phase 2).
5. Should **sellers** be allowed to set prices / adjust stock, or admin-only? (affects authz — Phase 4/3).
6. Fiscal **invoice numbering** requirements (Phase 8).
7. Multi-branch on the horizon? (would change schema — decide before Phase 3).
