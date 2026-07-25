namespace NsStore.Domain.Common;

/// <summary>
/// Base shape for master/transactional data: identity, audit trail and soft delete.
/// </summary>
public abstract class AuditableEntity
{
    public long Id { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>Null = active. Set to soft-delete the row.</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    public bool IsDeleted => DeletedAt is not null;
}
