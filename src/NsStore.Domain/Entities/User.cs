using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Domain.Entities;

public class User : AuditableEntity
{
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? MotherLastName { get; set; }
    public UserRole Role { get; set; } = UserRole.Seller;

    /// <summary>
    /// Home branch. NOT NULL for every role — an admin has one too; what sets them apart is being
    /// allowed to switch the active branch, not lacking a home one.
    /// </summary>
    public long BranchId { get; set; }

    public Branch Branch { get; set; } = null!;

    /// <summary>Disabled users cannot log in (legacy <c>us_enable</c>).</summary>
    public bool IsActive { get; set; } = true;

    public List<RefreshToken> RefreshTokens { get; set; } = [];

    public string FullName =>
        string.Join(' ', new[] { FirstName, LastName, MotherLastName }
            .Where(p => !string.IsNullOrWhiteSpace(p)));
}
