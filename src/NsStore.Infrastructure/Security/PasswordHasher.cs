using System.Security.Cryptography;
using NsStore.Application.Common.Interfaces;

namespace NsStore.Infrastructure.Security;

/// <summary>
/// PBKDF2-HMAC-SHA256 with a per-password random salt. Format:
/// <c>pbkdf2-sha256$&lt;iterations&gt;$&lt;saltBase64&gt;$&lt;hashBase64&gt;</c>.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const string Prefix = "pbkdf2-sha256";
    private const int Iterations = 210_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    public bool Verify(string password, string hash)
    {
        var parts = hash.Split('$');
        if (parts.Length != 4 || parts[0] != Prefix || !int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
