namespace NsStore.Domain.Enums;

public enum UserRole
{
    Admin,
    Seller
}

public enum ClientType
{
    Individual,
    Company
}

public enum InvoiceType
{
    WithInvoice,
    WithoutInvoice
}

public enum PaymentStatus
{
    Paid,
    Credit
}

public enum OrderStatus
{
    Pending,
    Delivered,
    Cancelled
}

/// <summary>
/// Transfers contribute <b>two</b> values, not one called <c>Transfer</c>. With a single value the
/// kardex would have to collapse to <c>SUM(quantity_delta)</c> and could no longer report
/// "dispatched 5" separately from "received 5" — and per branch those are two distinct physical
/// events at two different counters. New values are appended, which is safe for the native
/// PostgreSQL enum mapping (by type, values by name).
/// </summary>
public enum MovementType
{
    Purchase,
    Sale,
    Adjustment,
    TransferIn,
    TransferOut
}

/// <summary>
/// Lifecycle of one physical unit. <c>Removed</c> is terminal like <c>Sold</c>: a written-off unit
/// keeps its number so it can never be re-registered.
/// </summary>
public enum ProductSerialStatus
{
    InStock,
    Sold,
    Removed
}

/// <summary>
/// What happened to a unit. Kept apart from <see cref="MovementType"/> on purpose: <c>Registered</c>
/// moves no quantity at all, and the kardex sums over <see cref="MovementType"/> — folding these in
/// would corrupt the in/out identity a transfer relies on.
/// </summary>
public enum SerialEventType
{
    Received,
    Registered,
    Sold,
    Removed,
    TransferredOut,
    TransferredIn
}
