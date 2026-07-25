using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NsStore.Application.Common.Interfaces;
using NsStore.Domain.Entities;

namespace NsStore.Infrastructure.Security;

public class TokenService(IOptions<JwtOptions> options, TimeProvider clock) : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public AccessToken CreateAccessToken(User user)
    {
        var now = clock.GetUtcNow();
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);
        var credentials = new SigningCredentials(SigningKey(_options.SigningKey), SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = credentials,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString("N"),
                [ClaimTypes.Name] = user.Username,
                [ClaimTypes.Role] = user.Role.ToString().ToLowerInvariant()
            }
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new AccessToken(token, expiresAt);
    }

    public IssuedRefreshToken CreateRefreshToken()
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        return new IssuedRefreshToken(raw, HashRefreshToken(raw), clock.GetUtcNow().AddDays(_options.RefreshTokenDays));
    }

    /// <summary>
    /// Refresh tokens are high-entropy random values, so a plain SHA-256 is enough to keep the
    /// database from holding anything usable.
    /// </summary>
    public string HashRefreshToken(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    internal static SymmetricSecurityKey SigningKey(string key) => new(Encoding.UTF8.GetBytes(key));
}
