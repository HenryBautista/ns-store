using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NsStore.Application.Common.Interfaces;
using NsStore.Domain.Common;

namespace NsStore.Infrastructure.Persistence;

/// <summary>Stamps <c>CreatedBy</c>/<c>CreatedAt</c>/<c>UpdatedAt</c> so no service has to remember to.</summary>
public class AuditInterceptor(ICurrentUser currentUser, TimeProvider clock) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = clock.GetUtcNow();
        var userId = currentUser.UserId;

        // Covers AuditableEntity and write-once entities such as InventoryMovement.
        foreach (var entry in context.ChangeTracker.Entries<IHasCreationAudit>())
        {
            if (entry.State is EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy ??= userId;
            }
            else if (entry.State is EntityState.Modified && entry.Entity is AuditableEntity auditable)
            {
                auditable.UpdatedAt = now;
            }
        }
    }
}
