# 03 — Frontend (React + TypeScript)

> React SPA built with Vite on Node 24. Code in English; **all user-facing copy in Spanish via
> i18n** (no hardcoded strings). Typed end-to-end against the API.

## 1. Stack

| Concern | Choice |
|---|---|
| Build/dev | **Vite** (Node 24) |
| Language | **TypeScript** (strict) |
| Routing | **React Router** |
| Server state / data fetching | **TanStack Query** (caching, mutations, invalidation) |
| Client state | React context + hooks (minimal; avoid Redux unless needed) |
| Forms + validation | **react-hook-form** + **zod** (share zod schemas with API DTO shapes) |
| i18n | **react-i18next** (`es-BO` default locale) |
| UI components | **MUI** *or* **shadcn/ui + Tailwind** (pick one; MUI is faster for CRUD-heavy admin UIs) |
| HTTP | `fetch` wrapper or Axios with interceptors (auth + refresh) |
| Tables | TanStack Table (sorting/paging/filtering) |
| Testing | Vitest + React Testing Library; Playwright for E2E |

## 2. Project structure

```
web/
├── src/
│   ├── app/                 # app shell, router, providers (Query, i18n, theme, auth)
│   ├── features/            # one folder per domain module
│   │   ├── auth/
│   │   ├── products/
│   │   ├── catalogs/        # trademarks, categories, warranty-terms, suppliers
│   │   ├── clients/
│   │   ├── pricing/
│   │   ├── purchases/
│   │   ├── stock/
│   │   ├── sales/           # POS + sales list + debts + payments
│   │   ├── kardex/
│   │   ├── orders/
│   │   ├── quotes/
│   │   ├── users/
│   │   └── dashboard/
│   ├── shared/              # api client, hooks, ui primitives, types, formatters
│   ├── locales/es-BO/       # translation JSON (all Spanish UI text)
│   └── main.tsx
├── index.html
└── vite.config.ts
```

Each `feature/` typically has: `api.ts` (typed calls + query keys), `components/`, `pages/`,
`hooks.ts`, `schema.ts` (zod), `types.ts`.

## 3. Authentication flow (JWT + refresh)

- **Access token** kept in memory (React context), never in `localStorage` (XSS safety).
- **Refresh token** is an httpOnly cookie the JS never reads.
- **HTTP interceptor:**
  1. Attach `Authorization: Bearer <accessToken>`.
  2. On `401`, call `POST /auth/refresh` (cookie sent automatically) → get new access token → retry
     the original request once. If refresh fails → redirect to `/login`.
- **On app load / hard refresh:** call `/auth/refresh` to restore the session (access token is gone
  from memory but the cookie persists).
- **Route guards:** `<RequireAuth>` and `<RequireRole role="admin">` wrappers. Hide admin-only nav
  (Users, Settings) for sellers — mirroring legacy `us_master` behavior.

## 4. Routing map

```
/login
/                         → Dashboard
/products                 /products/new /products/:id
/catalogs/trademarks · /catalogs/categories · /catalogs/warranty-terms · /suppliers
/clients                  /clients/new /clients/:id
/pricing                  → set sale prices (suggestion + manual)
/purchases                /purchases/new /purchases/:id
/stock                    → stock levels; /stock/adjustments (admin)
/sales/new                → POS (cart)
/sales                    → sales list + debts tab + cobros
/sales/:id                → sale detail + payments + reprint note
/sales/by-client
/kardex
/orders                   (create/edit/search, owner-aware)
/quotes                   (create/edit/search, owner-aware)
/users                    (admin only)
/settings                 (admin only)
```

## 5. Key screens (mapped to legacy)

- **Dashboard** — tiles: sales by date range, purchases, stock, price list, **debts** (with
  register-payment action), orders, quotes; report buttons. Mirrors `Main_View`.
- **POS (`/sales/new`)** — client picker + product search + invoice toggle + quantity (with live
  stock check) → cart → totals → payment status + amount paid → confirm → warranty note. Blocks on
  insufficient stock / missing client / empty cart.
- **Sales / cobros (`/sales`, `/sales/:id`)** — list, search by client, detail with cart + payment
  history, register installment, reprint warranty note.
- **Pricing (`/pricing`)** — pick product → show suggestion (last cost +margin, +VAT) → set both
  prices.
- **Purchases (`/purchases/new`)** — supplier + product + unit price + quantity → cart → invoice
  type + payment status → confirm (increments stock).
- **Orders/Quotes** — CRUD with owner-based edit/delete permissions; advance ≤ price validation.

## 6. Internationalization

- Default and only locale in v1: **`es-BO`** (Spanish, Bolivia). Structure allows adding locales later.
- **Every visible string** comes from `locales/es-BO/*.json` via `t('key')`. No Spanish literals in
  components. Enum values from the API (camelCase: `withInvoice`, `credit`, `admin`, …) are mapped to
  Spanish labels in a central dictionary.
- **Formatting:** currency (BOB, from settings), dates, and numbers via `Intl` with the `es-BO`
  locale. Amounts to 2 decimals.
- Example enum → label map (API returns camelCase):
  - `withInvoice → "Con factura"`, `withoutInvoice → "Sin factura"`
  - `paid → "Contado / Pagado"`, `credit → "Crédito"`
  - `admin → "Administrador"`, `seller → "Vendedor"`
  - `pending → "Pendiente"`, `delivered → "Entregado"`

## 7. UX conventions
- Consistent data tables with server-side search/paging.
- Optimistic UI only where safe; otherwise show pending state and rely on TanStack Query invalidation.
- Confirmations for destructive actions (delete product, void).
- Inline stock warnings in POS (available quantity, "insufficient stock").
- Accessible components (labels, keyboard nav), responsive layout.
