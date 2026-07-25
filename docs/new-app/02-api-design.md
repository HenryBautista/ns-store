# 02 — API design (REST)

> ASP.NET Core Web API, JSON, versioned under `/api/v1`. Auth via JWT bearer (access token) +
> httpOnly refresh cookie. All responses locale-agnostic (codes/IDs/numbers); the SPA renders
> Spanish. Errors use RFC 7807 ProblemDetails with a stable `errorCode`.

## Conventions

- Base path: `/api/v1`.
- Auth: `Authorization: Bearer <access_token>` on all endpoints except `/auth/login` and `/auth/refresh`.
- Collections support `?search=`, `?page=`, `?pageSize=`, `?sort=` where relevant; return
  `{ items, page, pageSize, total }`.
- Money as JSON numbers with 2 decimals; dates/timestamps ISO-8601.
- Mutations return the created/updated resource. Idempotent where possible.
- Roles: `admin`, `seller`. Ownership rules noted per resource.

## Error model

```json
{
  "type": "https://nsstore/errors/insufficient-stock",
  "title": "Insufficient stock",
  "status": 409,
  "errorCode": "INSUFFICIENT_STOCK",
  "detail": "Product 42 requested 5, available 3",
  "traceId": "..."
}
```
Common `errorCode`s: `VALIDATION_ERROR`, `UNAUTHORIZED`, `FORBIDDEN`, `NOT_FOUND`,
`INSUFFICIENT_STOCK`, `CONCURRENCY_CONFLICT`, `DUPLICATE_USERNAME`, `PAYMENT_EXCEEDS_BALANCE`,
`ADVANCE_EXCEEDS_PRICE`.

---

## Authentication

| Method | Path | Body / notes | Result |
|---|---|---|---|
| POST | `/auth/login` | `{ username, password }` | `{ accessToken, user }` + sets refresh cookie (httpOnly). |
| POST | `/auth/refresh` | (refresh cookie) | new `{ accessToken }`, rotates refresh cookie. |
| POST | `/auth/logout` | (refresh cookie) | revokes refresh token family, clears cookie. |
| GET | `/auth/me` | — | current user profile + role. |

- Login fails (`401`, `INVALID_CREDENTIALS`) if user not found, password mismatch, or `is_active = false`.

---

## Users (admin only)

| Method | Path | Notes |
|---|---|---|
| GET | `/users` | list (search by name/username). |
| POST | `/users` | create `{ username, password, firstName, lastName, motherLastName }`. Defaults role=`seller`, active=true. `409 DUPLICATE_USERNAME` if taken. |
| GET | `/users/{id}` | detail. |
| PUT | `/users/{id}` | update profile/credentials. |
| PATCH | `/users/{id}/status` | `{ isActive }` enable/disable. |
| PATCH | `/users/{id}/role` | `{ role }` (admin only). |

---

## Catalogs (trademarks, categories, warranty-terms, suppliers)

Uniform CRUD per catalog (`/trademarks`, `/categories`, `/warranty-terms`, `/suppliers`):

| Method | Path | Notes |
|---|---|---|
| GET | `/{catalog}` | list + `?search=`. |
| POST | `/{catalog}` | create. |
| GET | `/{catalog}/{id}` | detail. |
| PUT | `/{catalog}/{id}` | update. |
| DELETE | `/{catalog}/{id}` | soft delete; `409` if referenced and hard rules apply. |

Fields: trademarks/categories `{ name }`; warranty-terms `{ description }`;
suppliers `{ name, phone, email }`.

---

## Products

| Method | Path | Notes |
|---|---|---|
| GET | `/products` | list + `?search=` (by name). Includes current stock and resolved trademark/category/warranty names. |
| POST | `/products` | `{ name, partNumber, description, serialNumber, trademarkId, categoryId, warrantyTermId }`. Prices start at 0. |
| GET | `/products/{id}` | detail. |
| PUT | `/products/{id}` | update descriptive fields (not prices). |
| DELETE | `/products/{id}` | soft delete. |
| GET | `/products/{id}/price-suggestion` | returns `{ lastCost, marginPct, vatPct, suggestedWithoutInvoice, suggestedWithInvoice }` from latest purchase cost + `app_settings`. |
| PUT | `/products/{id}/prices` | `{ priceWithInvoice, priceWithoutInvoice }` set sale prices (pricing module). |

---

## Inventory / stock

| Method | Path | Notes |
|---|---|---|
| GET | `/stock` | current stock levels per product + `?search=`. |
| GET | `/products/{id}/movements` | inventory ledger for a product. |
| POST | `/stock/adjustments` | `{ productId, quantityDelta, notes }` manual adjustment (admin) → writes a movement + updates level. |
| GET | `/kardex` | per-product summary `{ productId, name, totalPurchased, totalSold, available }` + `?search=`. |

---

## Purchases

| Method | Path | Notes |
|---|---|---|
| GET | `/purchases` | list (with supplier/user names) + `?search=`, `?from=`, `?to=`. |
| POST | `/purchases` | create (transactional). Body: `{ purchaseDate, supplierId, invoiceType, paymentStatus, items:[{ productId, quantity, unitPrice }] }`. Totals computed server-side. Increments stock. |
| GET | `/purchases/{id}` | header + items. |

---

## Sales (POS) & payments

| Method | Path | Notes |
|---|---|---|
| GET | `/sales` | list (with client/user names, balance) + `?search=` (client), `?from=`, `?to=`, `?status=`. |
| POST | `/sales` | create (transactional). Body: `{ saleDate, clientId, invoiceType, paymentStatus, initialPaid, items:[{ productId, quantity }] }`. Server prices each item by `invoiceType`, validates + decrements stock, records initial payment. `409 INSUFFICIENT_STOCK` on shortage. Returns sale + data needed to render the warranty note. |
| GET | `/sales/{id}` | header + items + payments + balance. |
| GET | `/sales/{id}/items` | cart lines. |
| POST | `/sales/{id}/payments` | `{ amount, paymentDate }` register installment. `409 PAYMENT_EXCEEDS_BALANCE`. Updates `total_paid`/status. |
| GET | `/clients/{id}/sales` | sales for a client. |
| GET | `/sales/debts` | credit sales with outstanding balance + `?search=` (client). (Legacy "No pagadas".) |

> Warranty note type is derived from `paymentStatus` (`paid` → standard note, `credit` → credit note),
> generated client-side or via a report endpoint (see Reports).

---

## Clients

| Method | Path | Notes |
|---|---|---|
| GET | `/clients` | list + `?search=` (name/last names). |
| POST | `/clients` | create; `type` = `individual` or `company` with the corresponding fields. |
| GET | `/clients/{id}` | detail (includes computed `fullName`). |
| PUT | `/clients/{id}` | update. |
| DELETE | `/clients/{id}` | soft delete. |

---

## Orders (encargos)

| Method | Path | Notes |
|---|---|---|
| GET | `/orders` | list + `?search=` (client/product/date). |
| POST | `/orders` | create; sets `owner_id = current user`. Validates `advance ≤ price`. |
| GET | `/orders/{id}` | detail. |
| PUT | `/orders/{id}` | update. **Ownership:** seller may edit only own; admin any. `403 FORBIDDEN` otherwise. |
| DELETE | `/orders/{id}` | **admin only** (legacy: sellers can't delete). |

## Quotes (cotizaciones)

Same shape as orders (`/quotes`): create sets owner; sellers edit only own; delete admin-only;
search by client/date.

---

## Settings & reports

| Method | Path | Notes |
|---|---|---|
| GET | `/settings` | business params (vat_rate, default_margin_pct, currency). |
| PUT | `/settings` | admin updates params. |
| GET | `/reports/{type}` | server-generated PDF or structured data for: `sales`, `purchases`, `stock`, `debts`, `price-list`, `sale-invoice/{saleId}`, `order/{id}`, `quote/{id}`. |

> Reports can be rendered as PDF server-side (QuestPDF) or as printable views client-side; decide in
> [roadmap](04-roadmap.md). The API at minimum exposes the structured data each report needs.

---

## Authorization matrix (summary)

| Area | seller | admin |
|---|---|---|
| Catalogs, products, clients, stock, purchases, sales, payments, kardex | ✅ | ✅ |
| Orders/quotes — create | ✅ | ✅ |
| Orders/quotes — edit | own only | any |
| Orders/quotes — delete | ❌ | ✅ |
| Users, settings, price changes, manual stock adjustments | ❌ | ✅ |

> Price changes and stock adjustments as admin-only is a **tightening** vs legacy (which let any
> user set prices). Confirm with the business; adjust the policy if sellers must set prices.
