using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NsStore.Domain.Entities;

namespace NsStore.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(160).IsRequired();
        builder.Property(p => p.PartNumber).HasMaxLength(80);
        builder.Property(p => p.Description).HasMaxLength(400);
        builder.Property(p => p.PriceWithInvoice).HasPrecision(12, 2);
        builder.Property(p => p.PriceWithoutInvoice).HasPrecision(12, 2);

        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.PartNumber);

        builder.HasOne(p => p.Trademark).WithMany().HasForeignKey(p => p.TrademarkId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Category).WithMany().HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.WarrantyTerm).WithMany().HasForeignKey(p => p.WarrantyTermId).OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_products_price_with_invoice_non_negative", "price_with_invoice >= 0");
            t.HasCheckConstraint("ck_products_price_without_invoice_non_negative", "price_without_invoice >= 0");
        });

        builder.HasQueryFilter(p => p.DeletedAt == null);
    }
}

public class StockLevelConfiguration : IEntityTypeConfiguration<StockLevel>
{
    public void Configure(EntityTypeBuilder<StockLevel> builder)
    {
        // Composite rather than surrogate: it spends no identity sequence, the PK index directly
        // serves "all stock in my branch", and branch_id leading gives the deterministic ordering
        // the FOR UPDATE acquisition relies on.
        builder.HasKey(s => new { s.BranchId, s.ProductId });

        builder.HasOne(s => s.Branch)
            .WithMany()
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Product)
            .WithMany(p => p.StockLevels)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Backs the FK now that product_id no longer leads the PK, and serves cross-branch reads.
        builder.HasIndex(s => s.ProductId);

        // Optimistic concurrency, now scoped per (branch, product): a conflict no longer spills
        // across branches.
        builder.Property(s => s.Version).IsConcurrencyToken();

        builder.ToTable(t => t.HasCheckConstraint("ck_stock_levels_quantity_non_negative", "quantity >= 0"));
    }
}

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.Property(m => m.UnitCost).HasPrecision(12, 2);
        builder.Property(m => m.ReferenceType).HasMaxLength(20);
        builder.Property(m => m.Notes).HasMaxLength(400);

        builder.HasOne(m => m.Branch)
            .WithMany()
            .HasForeignKey(m => m.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Product)
            .WithMany()
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.ProductId, m.CreatedAt });
        builder.HasIndex(m => new { m.BranchId, m.ProductId, m.CreatedAt });
        builder.HasIndex(m => new { m.ReferenceType, m.ReferenceId });

        builder.ToTable(t => t.HasCheckConstraint("ck_inventory_movements_quantity_delta_not_zero", "quantity_delta <> 0"));
    }
}
