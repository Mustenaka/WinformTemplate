using System.Security.Cryptography;
using System.Text;

namespace WinformTemplate.Tools.Encryption;

/// <summary>
/// MD5加密辅助类
/// </summary>
public static class MD5Helper
{
    /// <summary>
    /// MD5加密（32位小写）
    /// </summary>
    /// <param name="input">待加密字符串</param>
    /// <returns>加密后的MD5字符串</returns>
    public static string Encrypt(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        using (var md5 = MD5.Create())
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// MD5加密（32位大写）
    /// </summary>
    /// <param name="input">待加密字符串</param>
    /// <returns>加密后的MD5字符串</returns>
    public static string EncryptUpper(string input)
    {
        return Encrypt(input).ToUpper();
    }

    /// <summary>
    /// 验证MD5
    /// </summary>
    /// <param name="input">原始字符串</param>
    /// <param name="hash">MD5哈希值</param>
    /// <returns>是否匹配</returns>
    public static bool Verify(string input, string hash)
    {
        var inputHash = Encrypt(input);
        return string.Equals(inputHash, hash, StringComparison.OrdinalIgnoreCase);
    }
}
