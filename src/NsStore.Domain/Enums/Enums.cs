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
