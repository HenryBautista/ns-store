using NsStore.Domain.Common;

namespace NsStore.Domain.Entities;

public class Trademark : AuditableEntity
{
    public string Name { get; set; } = null!;
}

public class Category : AuditableEntity
{
    public string Name { get; set; } = null!;
}

public class WarrantyTerm : AuditableEntity
{
    /// <summary>Free text as entered by the business, e.g. "6 MESES", "1 AÑO", "SIN GARANTÍA".</summary>
    public string Description { get; set; } = null!;
}

public class Supplier : AuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
