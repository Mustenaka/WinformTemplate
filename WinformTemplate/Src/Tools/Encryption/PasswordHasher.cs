using System.Security.Cryptography;
using WinformTemplate.Logger;

namespace WinformTemplate.Tools.Encryption;

public static class PasswordHasher
{
    private const string Prefix = "PBKDF2";
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return string.Empty;
        }

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string? storedHash, out bool needsRehash)
    {
        needsRehash = false;

        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        if (storedHash.StartsWith($"{Prefix}$", StringComparison.Ordinal))
        {
            return VerifyPbkdf2(password, storedHash);
        }

        var legacyMatch = MD5Helper.Verify(password, storedHash);
        needsRehash = legacyMatch;
        return legacyMatch;
    }

    private static bool VerifyPbkdf2(string password, string storedHash)
    {
        var parts = storedHash.Split('$');
        if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException ex)
        {
            Debug.Warn($"密码哈希格式无效: {ex.Message}");
            return false;
        }
    }
}
