using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NsStore.Domain.Entities;

namespace NsStore.Infrastructure.Persistence.Configurations;

public class TrademarkConfiguration : IEntityTypeConfiguration<Trademark>
{
    public void Configure(EntityTypeBuilder<Trademark> builder)
    {
        builder.Property(t => t.Name).HasMaxLength(120).IsRequired();
        builder.HasIndex(t => t.Name);
        builder.HasQueryFilter(t => t.DeletedAt == null);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(120).IsRequired();
        builder.HasIndex(c => c.Name);
        builder.HasQueryFilter(c => c.DeletedAt == null);
    }
}

public class WarrantyTermConfiguration : IEntityTypeConfiguration<WarrantyTerm>
{
    public void Configure(EntityTypeBuilder<WarrantyTerm> builder)
    {
        builder.Property(w => w.Description).HasMaxLength(120).IsRequired();
        builder.HasIndex(w => w.Description);
        builder.HasQueryFilter(w => w.DeletedAt == null);
    }
}

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.Property(s => s.Name).HasMaxLength(120).IsRequired();
        builder.Property(s => s.Phone).HasMaxLength(40);
        builder.Property(s => s.Email).HasMaxLength(120);
        builder.HasIndex(s => s.Name);
        builder.HasQueryFilter(s => s.DeletedAt == null);
    }
}

public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.HasKey(s => s.Key);
        builder.Property(s => s.Key).HasMaxLength(60);
        builder.Property(s => s.Value).HasMaxLength(200).IsRequired();
    }
}
