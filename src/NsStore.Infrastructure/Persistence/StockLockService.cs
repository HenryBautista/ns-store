using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common.Interfaces;

namespace NsStore.Infrastructure.Persistence;

/// <summary>
/// Serializes concurrent stock changes with <c>SELECT ... FOR UPDATE</c>. Ids are locked in a
/// stable order so two carts touching the same products cannot deadlock each other.
/// </summary>
public class StockLockService(AppDbContext db) : IStockLockService
{
    public async Task LockAsync(IReadOnlyCollection<long> productIds, CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0 || !db.Database.IsNpgsql())
        {
            return;
        }

        var ordered = productIds.Distinct().OrderBy(id => id).ToArray();
        await db.Database.ExecuteSqlRawAsync(
            "SELECT 1 FROM stock_levels WHERE product_id = ANY({0}) ORDER BY product_id FOR UPDATE",
            [ordered],
            cancellationToken);
    }
}
