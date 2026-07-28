using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NsStore.Domain.Entities;

namespace NsStore.Infrastructure.Persistence.Configurations;

public class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.Property(t => t.Number).HasMaxLength(24).IsRequired();
        builder.Property(t => t.Notes).HasMaxLength(400);

        builder.HasOne(t => t.OriginBranch)
            .WithMany()
            .HasForeignKey(t => t.OriginBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.DestinationBranch)
            .WithMany()
            .HasForeignKey(t => t.DestinationBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Items)
            .WithOne(i => i.Transfer)
            .HasForeignKey(i => i.TransferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.TransferDate);
        builder.HasIndex(t => new { t.OriginBranchId, t.TransferDate });
        builder.HasIndex(t => new { t.DestinationBranchId, t.TransferDate });
        builder.HasIndex(t => new { t.OriginBranchId, t.BranchSequence }).IsUnique();
        builder.HasIndex(t => t.Number).IsUnique();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_stock_transfers_branches_differ", "origin_branch_id <> destination_branch_id");
            t.HasCheckConstraint("ck_stock_transfers_total_quantity_positive", "total_quantity > 0");
        });

        // Filter on the header, like Sale; the items carry none of their own, like SaleItem.
        builder.HasQueryFilter(t => t.DeletedAt == null);
    }
}

public class StockTransferItemConfiguration : IEntityTypeConfiguration<StockTransferItem>
{
    public void Configure(EntityTypeBuilder<StockTransferItem> builder)
    {
        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.ProductId);

        builder.ToTable(t => t.HasCheckConstraint("ck_stock_transfer_items_quantity_positive", "quantity > 0"));
    }
}
