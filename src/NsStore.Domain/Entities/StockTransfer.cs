using NsStore.Domain.Common;

namespace NsStore.Domain.Entities;

/// <summary>
/// Moves units between two branches in one transaction: origin is decremented, destination is
/// incremented, and two ledger entries are written.
/// </summary>
/// <remarks>
/// There is deliberately no "in transit" state and therefore no status enum — a transfer is
/// immediate. It is also immutable, like a sale: a mistake is corrected with a reverse transfer,
/// never an edit. The correlative comes from the <b>origin</b> branch.
/// </remarks>
public class StockTransfer : AuditableEntity
{
    public DateOnly TransferDate { get; set; }

    public long OriginBranchId { get; set; }
    public Branch OriginBranch { get; set; } = null!;

    public long DestinationBranchId { get; set; }
    public Branch DestinationBranch { get; set; } = null!;

    public string Number { get; set; } = null!;
    public long BranchSequence { get; set; }

    public int TotalQuantity { get; set; }
    public string? Notes { get; set; }

    public List<StockTransferItem> Items { get; set; } = [];
}

public class StockTransferItem
{
    public long Id { get; set; }
    public long TransferId { get; set; }
    public StockTransfer Transfer { get; set; } = null!;
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
}
