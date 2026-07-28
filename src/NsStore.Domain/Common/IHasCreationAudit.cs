namespace NsStore.Domain.Common;

/// <summary>
/// Creation audit trail for entities that are written once and never updated — the inventory
/// ledger being the case that does not fit <see cref="AuditableEntity"/>.
/// Lets the interceptor stamp them too, instead of every service remembering to.
/// </summary>
public interface IHasCreationAudit
{
    long? CreatedBy { get; set; }
    DateTimeOffset CreatedAt { get; set; }
}
