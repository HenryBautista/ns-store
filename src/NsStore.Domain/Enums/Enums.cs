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

public enum MovementType
{
    Purchase,
    Sale,
    Adjustment
}
