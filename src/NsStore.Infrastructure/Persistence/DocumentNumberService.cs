using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common.Interfaces;
using NsStore.Domain.Entities;

namespace NsStore.Infrastructure.Persistence;

/// <summary>
/// Increments the branch's counter column and returns the new value, enlisted in the ambient
/// transaction so a rolled-back document leaves no gap in the series.
/// </summary>
/// <remarks>
/// Unlike <see cref="StockLockService"/> this cannot no-op on non-Npgsql providers — the tests need
/// real numbers, and numbering is the piece most likely to carry an off-by-one. So it branches by
/// provider inside one class: one implementation, no test double, and the logic genuinely exercised
/// by the suite.
/// </remarks>
public class DocumentNumberService(AppDbContext db) : IDocumentNumberService
{
    public async Task<long> NextAsync(long branchId, DocumentKind kind, CancellationToken cancellationToken = default)
    {
        var column = kind switch
        {
            DocumentKind.Sale => "sale_sequence",
            DocumentKind.Purchase => "purchase_sequence",
            DocumentKind.Transfer => "transfer_sequence",
            DocumentKind.Receipt => "receipt_sequence",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown document kind")
        };

        if (db.Database.IsNpgsql())
        {
            // UPDATE ... RETURNING takes the row lock implicitly: no lost update, and no separate
            // SELECT ... FOR UPDATE. Different branches never contend.
            // The column name is chosen from the closed set above, never from caller input.
            // The "Value" alias is what EF requires to shape a scalar SqlQuery result.
            var next = await db.Database
                .SqlQueryRaw<long>(
                    $"UPDATE branches SET {column} = {column} + 1 WHERE id = {{0}} RETURNING {column} AS \"Value\"",
                    branchId)
                .ToListAsync(cancellationToken);

            return next.Count > 0
                ? next[0]
                : throw new InvalidOperationException($"Branch {branchId} does not exist");
        }

        // SQLite serialises writes, so loading, incrementing and saving is correct there.
        var branch = await db.Branches.FirstOrDefaultAsync(b => b.Id == branchId, cancellationToken)
            ?? throw new InvalidOperationException($"Branch {branchId} does not exist");

        var value = kind switch
        {
            DocumentKind.Sale => ++branch.SaleSequence,
            DocumentKind.Purchase => ++branch.PurchaseSequence,
            DocumentKind.Receipt => ++branch.ReceiptSequence,
            _ => ++branch.TransferSequence
        };

        await db.SaveChangesAsync(cancellationToken);
        return value;
    }
}
