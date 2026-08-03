using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NsStore.Domain.Entities;

namespace NsStore.Infrastructure.Persistence.Configurations;

public class ProductSerialConfiguration : IEntityTypeConfiguration<ProductSerial>
{
    public void Configure(EntityTypeBuilder<ProductSerial> builder)
    {
        builder.Property(s => s.SerialNumber).HasMaxLength(80).IsRequired();

        builder.HasOne(s => s.Product)
            .WithMany(p => p.Serials)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Branch)
            .WithMany()
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.PurchaseItem)
            .WithMany(i => i.Serials)
            .HasForeignKey(s => s.PurchaseItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SaleItem)
            .WithMany(i => i.Serials)
            .HasForeignKey(s => s.SaleItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unconditional, because this entity has no soft delete: a number, once used, is spent for
        // good. The migration adds a lower(serial_number) index beside it for the case-insensitive
        // rule; this one is exact-case and is the only constraint the SQLite test suite gets, since
        // EnsureCreated never builds raw-SQL indexes.
        builder.HasIndex(s => s.SerialNumber).IsUnique();

        // Serves the POS picker and the "how many units here are identified" count that decides how
        // many serials a sale must name.
        builder.HasIndex(s => new { s.ProductId, s.BranchId, s.Status });
        builder.HasIndex(s => s.SaleItemId);
        builder.HasIndex(s => s.PurchaseItemId);

        builder.Property(s => s.Version).IsConcurrencyToken();

        // No check constraint tying status to sale_item_id: on SQLite the enum is an INTEGER, so the
        // predicate would be silently always-true and the coverage illusory. MarkSold enforces it,
        // and the domain tests actually exercise that.
        // No HasQueryFilter either — see the remarks on ProductSerial.
    }
}

public class ProductSerialEventConfiguration : IEntityTypeConfiguration<ProductSerialEvent>
{
    public void Configure(EntityTypeBuilder<ProductSerialEvent> builder)
    {
        builder.Property(e => e.ReferenceType).HasMaxLength(20);
        builder.Property(e => e.Notes).HasMaxLength(400);

        builder.HasOne(e => e.Serial)
            .WithMany(s => s.Events)
            .HasForeignKey(e => e.SerialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Branch)
            .WithMany()
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.SerialId, e.CreatedAt });

        // Lets a transfer note list the serials it moved, which is the read the ledger exists for.
        builder.HasIndex(e => new { e.ReferenceType, e.ReferenceId });
    }
}
