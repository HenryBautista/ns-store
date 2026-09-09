# Sales

The most easily broken part of the system, because a sale is the one operation that has to move
money and stock together and stay correct under two cashiers at once.

## A sale is one unit of work

`IAppDbContext.ExecuteInTransactionAsync` wraps `IStockLockService.LockAsync`, plus the optimistic
`Version` check on each `StockLevel`. Three things about that shape are load-bearing:

- **`LockAsync` takes the ids in ascending order.** `SELECT … FOR UPDATE` on `stock_levels` sorted
  by id is what prevents a deadlock between two transactions that want the same two products in
  opposite order. It is not a stylistic detail.
- **The transaction is safe to nest and is retried whole** under the Npgsql execution strategy, so
  **the wrapped action must be idempotent**. Anything with a side effect outside the database
  belongs outside the transaction.
- Overselling surfaces as `409 INSUFFICIENT_STOCK`, raised by `StockLevel.Apply`, not by a check
  in the service.

None of this exists under SQLite: the test suite runs with a `NoOpStockLock`, so **the locking is
not covered by any test**. See `.agent/decisions.md#d-07` and the traps section of
`.agent/findings.md`.

## Stock moves through the ledger, never around it

Every stock change writes an `InventoryMovement` — the source of truth — **and** updates the
per-product, per-branch `StockLevel`, which is a cache. Never assign `StockLevel.Quantity`: call
`Apply`, which enforces non-negative stock and bumps the `Version` concurrency token. A direct
assignment desynchronises the two representations silently, and the cache then lies without
anything failing. See `.agent/decisions.md#d-06`.

Stock only moves through purchases, sales, admin adjustments and branch transfers.

## Dual price

Each product stores a price with and without invoice. The sale's `InvoiceType` selects which price
**every line** uses — it is a property of the sale, not of the line.

Price suggestion is `withoutInvoice = lastPurchaseCost × (1 + margin)` and
`withInvoice = withoutInvoice × (1 + vat)`. Margin and VAT come from `app_settings`, seeded at the
legacy 30 % and 16 %. **Never hardcode them**: the SPA reads the same two settings, and a constant
here is a second copy of the rule that will drift.

## Credit and collection

A credit sale keeps a balance; each installment is a `Payment` row, and the sale flips to `paid`
at zero balance. There are three ways to collect and only one issues a numbered
`PaymentReceipt` — see `DT-07` in `.agent/findings.md` before assuming a receipt exists.

## Child tables have no soft-delete filter

`SaleItem` has no query filter of its own, so any query that reaches it must go through the
filtered `Sale`. Querying `SaleItem` directly returns lines belonging to deleted sales. The kardex
projection in `InventoryService` is the example to copy.
