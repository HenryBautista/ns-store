using System.ComponentModel.DataAnnotations;

namespace NsStore.Infrastructure.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = "nsstore-api";

    [Required]
    public string Audience { get; set; } = "nsstore-web";

    /// <summary>Signing key — supplied by environment variable / user-secrets, never committed.</summary>
    [Required, MinLength(32)]
    public string SigningKey { get; set; } = null!;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 14;
}
