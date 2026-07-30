using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NsStore.Domain.Entities;

namespace NsStore.Infrastructure.Persistence.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.Property(b => b.Code).HasMaxLength(8).IsRequired();
        builder.Property(b => b.Name).HasMaxLength(120).IsRequired();
        builder.Property(b => b.Address).HasMaxLength(200);
        builder.Property(b => b.Phone).HasMaxLength(40);

        // Case-insensitive uniqueness is a lower(code) partial unique index created in raw SQL by
        // the AddBranches migration; this one serves lookups.
        builder.HasIndex(b => b.Code);

        // No HasQueryFilter on purpose — see the remarks on the Branch entity. A filtered principal
        // would propagate through the required Sale.Branch / StockLevel.Branch navigations and
        // silently drop rows out of reports.
    }
}
