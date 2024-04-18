using System.Security.Cryptography;
using System.Text;

namespace WinformTemplate.Tools.Encryption.Symmetric.DESHelper;

public class DESHelper : ISymmetricEncryption
{
    /// <summary>
    /// DES加密
    /// </summary>
    /// <param name="plainText">加密字符串</param>
    /// <param name="key">解密Key</param>
    /// <returns>加密结果</returns>
    public static byte[] EncryptDES(string plainText, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

        using (var cryptoProvider = DES.Create())
        {
            cryptoProvider.Key = keyBytes;
            cryptoProvider.Mode = CipherMode.ECB;

            using (ICryptoTransform encryptor = cryptoProvider.CreateEncryptor())
            {
                return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            }
        }
    }

    /// <summary>
    /// DES解密
    /// </summary>
    /// <param name="encryptedData">加密数据</param>
    /// <param name="key">解密Key</param>
    /// <returns>解密结果</returns>
    public static string DecryptDES(byte[] encryptedData, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);

        using (var cryptoProvider = DES.Create())
        {
            cryptoProvider.Key = keyBytes;
            cryptoProvider.Mode = CipherMode.ECB;

            using (ICryptoTransform decryptor = cryptoProvider.CreateDecryptor())
            {
                byte[] plainBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                return Encoding.UTF8.GetString(plainBytes);
            }
        }
    }
}