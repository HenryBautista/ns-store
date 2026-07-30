using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NsStore.Domain.Entities;

namespace NsStore.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(160).IsRequired();
        builder.Property(c => c.LastName).HasMaxLength(80);
        builder.Property(c => c.MotherLastName).HasMaxLength(80);
        builder.Property(c => c.Ci).HasMaxLength(30);
        builder.Property(c => c.Nit).HasMaxLength(30);
        builder.Property(c => c.Phone).HasMaxLength(40);
        builder.Property(c => c.Email).HasMaxLength(120);
        builder.Property(c => c.City).HasMaxLength(80);
        builder.Property(c => c.Address).HasMaxLength(200);
        builder.Property(c => c.ContactName).HasMaxLength(120);
        builder.Ignore(c => c.FullName);

        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.LastName);
        builder.HasIndex(c => c.Nit);

        // A person's CI is unique, but only among live rows: a soft-deleted client must not
        // reserve its CI forever. NIT stays non-unique — a person and their company can share one.
        builder.HasIndex(c => c.Ci).IsUnique().HasFilter("ci IS NOT NULL AND deleted_at IS NULL");

        builder.HasQueryFilter(c => c.DeletedAt == null);
    }
}

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.Property(p => p.TotalAmount).HasPrecision(12, 2);

        builder.HasOne(p => p.Branch)
            .WithMany()
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Supplier)
            .WithMany()
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Items)
            .WithOne(i => i.Purchase)
            .HasForeignKey(i => i.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.Number).HasMaxLength(24).IsRequired();

        builder.HasIndex(p => p.PurchaseDate);
        builder.HasIndex(p => new { p.BranchId, p.PurchaseDate });
        builder.HasIndex(p => new { p.BranchId, p.BranchSequence }).IsUnique();
        builder.HasIndex(p => p.Number).IsUnique();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_purchases_total_quantity_positive", "total_quantity > 0");
            t.HasCheckConstraint("ck_purchases_total_amount_non_negative", "total_amount >= 0");
        });

        builder.HasQueryFilter(p => p.DeletedAt == null);
    }
}

public class PurchaseItemConfiguration : IEntityTypeConfiguration<PurchaseItem>
{
    public void Configure(EntityTypeBuilder<PurchaseItem> builder)
    {
        builder.Property(i => i.UnitPrice).HasPrecision(12, 2);
        builder.Property(i => i.Subtotal).HasPrecision(12, 2);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.ProductId);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_purchase_items_quantity_positive", "quantity > 0");
            t.HasCheckConstraint("ck_purchase_items_unit_price_non_negative", "unit_price >= 0");
        });
    }
}

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.Property(s => s.TotalAmount).HasPrecision(12, 2);
        builder.Property(s => s.TotalPaid).HasPrecision(12, 2);
        builder.Ignore(s => s.Balance);

        builder.HasOne(s => s.Branch)
            .WithMany()
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Client)
            .WithMany()
            .HasForeignKey(s => s.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Items)
            .WithOne(i => i.Sale)
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Payments)
            .WithOne(p => p.Sale)
            .HasForeignKey(p => p.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.Number).HasMaxLength(24).IsRequired();

        builder.HasIndex(s => s.SaleDate);
        builder.HasIndex(s => new { s.PaymentStatus, s.SaleDate });
        builder.HasIndex(s => new { s.BranchId, s.SaleDate });

        // Belt and braces against a counter bug; the folio is globally unique because its prefix is.
        builder.HasIndex(s => new { s.BranchId, s.BranchSequence }).IsUnique();
        builder.HasIndex(s => s.Number).IsUnique();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_sales_total_quantity_positive", "total_quantity > 0");
            t.HasCheckConstraint("ck_sales_total_amount_non_negative", "total_amount >= 0");
            t.HasCheckConstraint("ck_sales_total_paid_within_total", "total_paid >= 0 AND total_paid <= total_amount");
        });

        builder.HasQueryFilter(s => s.DeletedAt == null);
    }
}

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.Property(i => i.UnitPrice).HasPrecision(12, 2);
        // Legacy declared this column as `bit`; it is money.
        builder.Property(i => i.Subtotal).HasPrecision(12, 2);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.ProductId);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_sale_items_quantity_positive", "quantity > 0");
            t.HasCheckConstraint("ck_sale_items_unit_price_non_negative", "unit_price >= 0");
        });
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.Amount).HasPrecision(12, 2);

        builder.HasOne(p => p.Branch)
            .WithMany()
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.SaleId, p.PaymentDate });
        builder.ToTable(t => t.HasCheckConstraint("ck_payments_amount_positive", "amount > 0"));
    }
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.ClientName).HasMaxLength(160).IsRequired();
        builder.Property(o => o.Phone).HasMaxLength(40);
        builder.Property(o => o.ProductDescription).HasMaxLength(400).IsRequired();
        builder.Property(o => o.Notes).HasMaxLength(400);
        builder.Property(o => o.Price).HasPrecision(12, 2);
        builder.Property(o => o.AdvanceAmount).HasPrecision(12, 2);
        builder.Ignore(o => o.Balance);

        builder.HasOne(o => o.Branch)
            .WithMany()
            .HasForeignKey(o => o.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Owner)
            .WithMany()
            .HasForeignKey(o => o.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => o.OrderDate);
        builder.HasIndex(o => o.OwnerId);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_orders_price_non_negative", "price >= 0");
            // The legacy enforced this only in the UI.
            t.HasCheckConstraint("ck_orders_advance_within_price", "advance_amount >= 0 AND advance_amount <= price");
        });

        builder.HasQueryFilter(o => o.DeletedAt == null);
    }
}

public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.Property(q => q.ClientName).HasMaxLength(160).IsRequired();
        builder.Property(q => q.Phone).HasMaxLength(40);
        builder.Property(q => q.Detail).HasMaxLength(1000).IsRequired();
        builder.Property(q => q.SupplierName).HasMaxLength(160);
        builder.Property(q => q.Price).HasPrecision(12, 2);

        builder.HasOne(q => q.Branch)
            .WithMany()
            .HasForeignKey(q => q.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.Owner)
            .WithMany()
            .HasForeignKey(q => q.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(q => q.QuoteDate);
        builder.HasIndex(q => q.OwnerId);

        builder.ToTable(t => t.HasCheckConstraint("ck_quotes_price_non_negative", "price >= 0"));
        builder.HasQueryFilter(q => q.DeletedAt == null);
    }
}
