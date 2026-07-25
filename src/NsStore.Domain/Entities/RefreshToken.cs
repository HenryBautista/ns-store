namespace NsStore.Domain.Entities;

public class RefreshToken
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>SHA-256 of the raw token. The raw value never touches the database.</summary>
    public string TokenHash { get; set; } = null!;

    /// <summary>Rotation family: reusing any revoked member revokes the whole family.</summary>
    public Guid FamilyId { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;
}
