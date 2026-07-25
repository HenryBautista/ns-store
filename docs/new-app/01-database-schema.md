# 01 — Data model (EF Core, code-first)

> The model is defined **code-first with EF Core** (the ORM). C# entities below use **PascalCase**;
> **EF Core migrations generate the PostgreSQL schema** — no hand-written DDL, no legacy `t_`/column
> prefixes, no snake_case. Money is `decimal(12,2)`; timestamps are `DateTimeOffset` (`timestamptz`);
> soft-delete via `DeletedAt`; audit via `CreatedBy`/`CreatedAt`/`UpdatedAt`. Dead/redundant legacy
> tables (`t_person`, `t_business`, `t_sale_price`) do not exist here.

## Modeling conventions

- **PK:** `long Id { get; set; }` → identity column.
- **Enums:** C# enums, mapped to PostgreSQL native enums via Npgsql (`MapEnum<T>()`), serialized to
  camelCase in JSON.
- **Money:** `decimal` with `HasPrecision(12, 2)`; non-negative via check constraints (`ToTable(t => t.HasCheckConstraint(...))`).
- **Soft delete:** `DateTimeOffset? DeletedAt`; a global query filter (`HasQueryFilter(e => e.DeletedAt == null)`) hides deleted rows; unique indexes are filtered on `DeletedAt IS NULL`.
- **Audit:** `long? CreatedBy`, `DateTimeOffset CreatedAt`, `DateTimeOffset? UpdatedAt` (via a base type / interceptor).
- **Concurrency:** `uint Version` mapped to Postgres `xmin` (`IsRowVersion()`), or an explicit version column on `StockLevel`.

A small base class captures the audit/soft-delete shape:

```csharp
public abstract class AuditableEntity
{
    public long Id { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }   // null = active
}
```

## Enums

```csharp
public enum UserRole      { Admin, Seller }
public enum ClientType    { Individual, Company }
public enum InvoiceType   { WithInvoice, WithoutInvoice }
public enum PaymentStatus { Paid, Credit }
public enum OrderStatus   { Pending, Delivered, Cancelled }
public enum MovementType  { Purchase, Sale, Adjustment }
```
> JSON serialization: camelCase strings (`admin`, `withInvoice`, `credit`, `pending`, `purchase`, …).

---

## Identity & security

```csharp
public class User : AuditableEntity
{
    public string Username { get; set; } = null!;      // unique (case-insensitive), active only
    public string PasswordHash { get; set; } = null!;  // Argon2/PBKDF2 — never plaintext
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? MotherLastName { get; set; }
    public UserRole Role { get; set; } = UserRole.Seller;
    public bool IsActive { get; set; } = true;         // disabled users cannot log in
}

public class RefreshToken
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public string TokenHash { get; set; } = null!;     // store hash, not the raw token
    public Guid FamilyId { get; set; }                 // rotation family (reuse detection)
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
```
Config: unique index on `lower(Username)` filtered by `DeletedAt IS NULL`. Maps legacy
`us_master → Role`, `us_enable → IsActive`.

---

## Catalogs

```csharp
public class Trademark : AuditableEntity { public string Name { get; set; } = null!; }
public class Category  : AuditableEntity { public string Name { get; set; } = null!; }

public class WarrantyTerm : AuditableEntity
{
    public string Description { get; set; } = null!;   // e.g. "6 MESES", "1 AÑO", "SIN GARANTÍA"
}

public class Supplier : AuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
```
Config: unique index on `lower(Name)` (filtered) for Trademark/Category/Supplier.

---

## Products & inventory

```csharp
public class Product : AuditableEntity
{
    public string Name { get; set; } = null!;
    public string? PartNumber { get; set; }
    public string? Description { get; set; }
    public string? SerialNumber { get; set; }

    public long? TrademarkId { get; set; }
    public Trademark? Trademark { get; set; }
    public long? CategoryId { get; set; }
    public Category? Category { get; set; }
    public long? WarrantyTermId { get; set; }
    public WarrantyTerm? WarrantyTerm { get; set; }

    public decimal PriceWithInvoice { get; set; }      // default 0, set in pricing module
    public decimal PriceWithoutInvoice { get; set; }   // default 0

    public StockLevel? StockLevel { get; set; }
}

// Current quantity cache — one row per product; NEVER deleted (can reach 0)
public class StockLevel
{
    public long ProductId { get; set; }                // PK = FK to Product
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }                  // CHECK >= 0
    public DateTimeOffset UpdatedAt { get; set; }
    public int Version { get; set; }                   // optimistic concurrency
}

// Ledger — source of truth for stock history / kardex
public class InventoryMovement
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public MovementType MovementType { get; set; }     // Purchase | Sale | Adjustment
    public int QuantityDelta { get; set; }             // + inbound, - outbound (≠ 0)
    public decimal? UnitCost { get; set; }             // for purchases → feeds pricing suggestion
    public string? ReferenceType { get; set; }         // "purchase" | "sale" | "manual"
    public long? ReferenceId { get; set; }             // Purchase.Id / Sale.Id
    public string? Notes { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
```
> **Kardex** = aggregate over `InventoryMovement` (Σ inbound purchases, Σ outbound sales, current = Σ delta).
> Improvement vs legacy: `StockLevel` row is never deleted; quantity can be 0.

---

## Clients

```csharp
public class Client : AuditableEntity
{
    public ClientType Type { get; set; }               // Individual | Company
    // Individual: Name = first name; Company: Name = legal/business name
    public string Name { get; set; } = null!;
    public string? LastName { get; set; }              // individual
    public string? MotherLastName { get; set; }        // individual
    public string? Ci { get; set; }                    // individual national ID
    public string? Nit { get; set; }                   // tax ID (both)
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }                  // company
    public string? Address { get; set; }               // company
    public string? ContactName { get; set; }           // company

    // Computed (not mapped): FullName = Name + LastName + MotherLastName
}
```
Single table + `Type` discriminator (legacy `cl_type`). Dead FKs `cl_person`/`cl_business` dropped.

---

## Purchases

```csharp
public class Purchase : AuditableEntity
{
    public DateOnly PurchaseDate { get; set; }
    public long SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public InvoiceType InvoiceType { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public int TotalQuantity { get; set; }             // CHECK > 0
    public decimal TotalAmount { get; set; }           // computed server-side
    public List<PurchaseItem> Items { get; set; } = new();
}

public class PurchaseItem
{
    public long Id { get; set; }
    public long PurchaseId { get; set; }
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }                  // CHECK > 0
    public decimal UnitPrice { get; set; }             // purchase cost
    public decimal Subtotal { get; set; }              // Quantity * UnitPrice
}
```

---

## Sales, payments

```csharp
public class Sale : AuditableEntity
{
    public DateOnly SaleDate { get; set; }
    public long ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public InvoiceType InvoiceType { get; set; }       // whole sale
    public PaymentStatus PaymentStatus { get; set; }
    public int TotalQuantity { get; set; }             // CHECK > 0
    public decimal TotalAmount { get; set; }
    public decimal TotalPaid { get; set; }             // CHECK 0 <= TotalPaid <= TotalAmount
    public List<SaleItem> Items { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
    // Computed (not mapped): Balance = TotalAmount - TotalPaid
    // PaymentStatus becomes Paid when TotalPaid == TotalAmount
}

public class SaleItem
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }                  // CHECK > 0
    public decimal UnitPrice { get; set; }             // price per InvoiceType
    public decimal Subtotal { get; set; }              // fixes legacy bug: was typed `bit`
}

// Installments / abonos — NEW (improvement): auditable credit payments
public class Payment
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public decimal Amount { get; set; }                // CHECK > 0
    public DateOnly PaymentDate { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
```
> `Sale.TotalPaid` = initial payment + Σ `Payment.Amount`, maintained transactionally.

---

## Orders & quotes

```csharp
public class Order : AuditableEntity
{
    public DateOnly OrderDate { get; set; }
    public string ClientName { get; set; } = null!;        // free text (may not be a catalog client)
    public string? Phone { get; set; }
    public string ProductDescription { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal AdvanceAmount { get; set; }             // CHECK AdvanceAmount <= Price
    public string? Notes { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public long OwnerId { get; set; }                      // edit permission by owner
    public User Owner { get; set; } = null!;
    // Computed: Balance = Price - AdvanceAmount
}

public class Quote : AuditableEntity
{
    public DateOnly QuoteDate { get; set; }
    public string ClientName { get; set; } = null!;
    public string? Phone { get; set; }
    public string Detail { get; set; } = null!;
    public decimal Price { get; set; }
    public string? SupplierName { get; set; }              // free text (legacy quirk kept)
    public long OwnerId { get; set; }
    public User Owner { get; set; } = null!;
}
```
> `AdvanceAmount <= Price` is a DB check constraint (legacy enforced it only in the UI).

---

## Settings

```csharp
public class AppSetting
{
    public string Key { get; set; } = null!;   // "vat_rate", "default_margin_pct", "currency"
    public string Value { get; set; } = null!;
    public long? UpdatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
// Seed: vat_rate=16, default_margin_pct=30, currency=BOB
```
> Replaces hardcoded 30% margin / 16% VAT. **Confirm the correct VAT rate with the business** (legacy doc 05 §5).

---

## Transactional invariants (enforced in application services, one DB transaction each)

| Operation | Steps |
|---|---|
| **Create sale** | validate stock per item (lock `StockLevel`) → insert `Sale` + `SaleItem`s → insert `InventoryMovement`s (Sale, negative) → decrement `StockLevel` → if paid, insert initial `Payment` + set status. |
| **Create purchase** | insert `Purchase` + `PurchaseItem`s → insert `InventoryMovement`s (Purchase, positive, with `UnitCost`) → upsert/increment `StockLevel`. |
| **Register payment** | insert `Payment` → recompute `Sale.TotalPaid` → set `PaymentStatus = Paid` when fully paid; reject if amount exceeds balance. |
| **Price suggestion** | latest `UnitCost` from `InventoryMovement` → `withoutInvoice = cost * (1 + margin)`, `withInvoice = withoutInvoice * (1 + vat)` using `AppSetting`. |

## Referential integrity policy
- **Soft delete** (`DeletedAt`) for master data (products, clients, suppliers, catalogs, users).
- Transactions (sales/purchases) not hard-deleted in normal operation; void via `DeletedAt` + audit.
- Cascade delete only from a header to its own line items (`Purchase → PurchaseItem`, `Sale → SaleItem`), configured in EF Core.
