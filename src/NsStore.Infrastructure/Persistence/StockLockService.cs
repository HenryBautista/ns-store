using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common.Interfaces;

namespace NsStore.Infrastructure.Persistence;

/// <summary>
/// Serializes concurrent stock changes with <c>SELECT ... FOR UPDATE</c>. Keys are locked in a
/// stable (branch, product) order so two carts — or a sale and a transfer — touching the same rows
/// cannot deadlock each other.
/// </summary>
public class StockLockService(AppDbContext db) : IStockLockService
{
    public async Task LockAsync(IReadOnlyCollection<StockKey> keys, CancellationToken cancellationToken = default)
    {
        if (keys.Count == 0 || !db.Database.IsNpgsql())
        {
            return;
        }

        // Every writer in the system orders by the same key, which is what makes an A→B and a
        // concurrent B→A transfer acquire in identical order.
        var ordered = keys.Distinct().OrderBy(k => k.BranchId).ThenBy(k => k.ProductId).ToArray();
        var branchIds = ordered.Select(k => k.BranchId).ToArray();
        var productIds = ordered.Select(k => k.ProductId).ToArray();

        // unnest(a, b) is the multi-array form that zips two parallel arrays into rows;
        // `(branch_id, product_id) = ANY (subquery)` is not valid syntax. FOR UPDATE OF s is
        // mandatory — a bare FOR UPDATE errors because the function alias k is not lockable.
        await db.Database.ExecuteSqlRawAsync(
            """
            SELECT 1
            FROM stock_levels s
            JOIN unnest({0}::bigint[], {1}::bigint[]) AS k(branch_id, product_id)
              ON s.branch_id = k.branch_id AND s.product_id = k.product_id
            ORDER BY s.branch_id, s.product_id
            FOR UPDATE OF s
            """,
            [branchIds, productIds],
            cancellationToken);
    }
}
