using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NsStore.Domain.Entities;

namespace NsStore.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Username).HasMaxLength(60).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(400).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(80).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(80).IsRequired();
        builder.Property(u => u.MotherLastName).HasMaxLength(80);
        builder.Ignore(u => u.FullName);

        // Case-insensitive uniqueness is enforced by a lower(username) partial unique index
        // created in the initial migration; this index serves lookups.
        builder.HasOne(u => u.Branch)
            .WithMany()
            .HasForeignKey(u => u.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.Username);
        builder.HasIndex(u => u.BranchId);
        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(t => t.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.HasIndex(t => t.FamilyId);

        builder.HasOne(t => t.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
