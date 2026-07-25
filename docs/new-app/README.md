# NS Store — New Web Application Plan

> Implementation plan for the ground-up rewrite of the legacy NS_Store WPF/SQL Server system
> as a modern web application. This plan is the source of truth for the new build; it consumes
> the reverse-engineered business logic documented in [`../`](../README.md) (Spanish).

## Language convention

- **All engineering artifacts in English**: source code, identifiers, database schema,
  API contracts, commit messages, and these planning documents.
- **Only end-user-facing text in Spanish**, delivered through **i18n** (locale files).
  No hardcoded UI strings.

## Naming & modeling conventions (modern, no legacy carry-over)

The legacy DB conventions are **not** carried over. Explicitly dropped:

- ❌ `t_` table prefixes and per-column prefixes/suffixes (`pr_`, `cl_`, …).
- ❌ Hand-written snake_case DDL.

Instead:

- ✅ **Code-first via EF Core** (the ORM): entities define the model, migrations generate the schema.
  No hand-authored SQL for the schema.
- ✅ **C# entities & properties in PascalCase** (`Product`, `PriceWithInvoice`, `SaleItem`).
- ✅ **JSON / API contracts in camelCase** (`priceWithInvoice`, `invoiceType`).
- ✅ **Enums as C# enums**, serialized to camelCase strings (`withInvoice`, `credit`, `admin`).
- ✅ Clean, unprefixed, descriptive names throughout.

## Confirmed technical decisions

| Decision | Choice | Rationale |
|---|---|---|
| Backend | **.NET 10** — ASP.NET Core Web API | Modern, LTS-track, strong typing, EF Core. |
| Frontend | **React + TypeScript** (Vite) | SPA; typed end-to-end. |
| Frontend runtime/tooling | **Node.js 24 LTS** | Latest LTS for build/dev tooling. |
| Database | **PostgreSQL** | Greenfield-friendly, `numeric`/`timestamptz`, great EF Core support. |
| Authentication | **JWT access token + refresh token** | Short-lived access token in memory; refresh token in httpOnly cookie. |
| v1 scope | **Parity + key improvements** | Replicate legacy behavior while fixing the structural debts. |
| Historical data | **No migration** — clean start | Manual load of master catalogs. |

### Key improvements included in v1 (vs. strict legacy parity)

1. **Inventory movement ledger** (`inventory_movements`) instead of mutating a single counter — real kardex and history.
2. **Payments/installments table** (`payments`) for credit-sale traceability.
3. **Parameterized tax (VAT) and margin** via `app_settings` — no hardcoded 30% / 16%.
4. **Soft delete** and **audit fields** (`created_by`, timestamps) everywhere.
5. **Money as `decimal`**, **timestamps as `timestamptz`**.
6. **Atomic transactions** for sale/purchase/payment operations.

## Plan documents

| Document | Contents |
|---|---|
| [00 — Architecture](00-architecture.md) | Solution structure, layers, cross-cutting concerns (auth, errors, logging, config, i18n). |
| [01 — Database schema](01-database-schema.md) | New PostgreSQL schema: tables, columns, enums, constraints, indexes. |
| [02 — API design](02-api-design.md) | REST endpoints per module, request/response contracts, auth flows. |
| [03 — Frontend](03-frontend.md) | React app structure, routing, state, forms, i18n, UI. |
| [04 — Roadmap](04-roadmap.md) | Phased delivery plan, milestones, definition of done. |

## Legacy → new naming map (quick reference)

C# entity names (PascalCase). EF Core maps them to the DB via migrations.

| Legacy (Spanish/DB) | New C# entity |
|---|---|
| `t_product` | `Product` |
| `t_trademark` | `Trademark` |
| `t_category` | `Category` |
| `t_warranty` | `WarrantyTerm` |
| `t_supplier` | `Supplier` |
| `t_client` | `Client` |
| `t_stock` | `StockLevel` + `InventoryMovement` |
| `t_purchase` / `t_purchase_product` | `Purchase` / `PurchaseItem` |
| `t_sale` / `t_sale_product` | `Sale` / `SaleItem` |
| (new) | `Payment` |
| `t_orders` | `Order` |
| `t_quotes` | `Quote` |
| `t_users` | `User` (+ `RefreshToken`) |
| `t_person`, `t_business`, `t_sale_price` | **dropped** (dead/redundant) |
| con factura / sin factura | `InvoiceType.WithInvoice` / `WithoutInvoice` |
| contado / crédito | `PaymentStatus.Paid` / `Credit` |
| master / normal | `UserRole.Admin` / `Seller` |
