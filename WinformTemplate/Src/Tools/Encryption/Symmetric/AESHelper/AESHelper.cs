using System.Security.Cryptography;
using System.Text;

namespace WinformTemplate.Tools.Encryption.Symmetric.AESHelper;

public class AESHelper : ISymmetricEncryption
{
    public static byte[] EncryptDES(string plainText, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

        using (var cryptoProvider = Aes.Create())
        {
            cryptoProvider.Key = keyBytes;

            cryptoProvider.GenerateIV();
            byte[] iv = cryptoProvider.IV;

            using (ICryptoTransform encryptor = cryptoProvider.CreateEncryptor())
            {
                byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                byte[] encryptedData = new byte[encryptedBytes.Length + iv.Length];
                Array.Copy(iv, 0, encryptedData, 0, iv.Length);
                Array.Copy(encryptedBytes, 0, encryptedData, iv.Length, encryptedBytes.Length);

                return encryptedData;
            }
        }
    }

    public static string DecryptDES(byte[] encryptedData, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);

        using (var cryptoProvider = Aes.Create())
        {
            int ivSize = cryptoProvider.BlockSize / 8;
            byte[] iv = new byte[ivSize];
            Array.Copy(encryptedData, 0, iv, 0, ivSize);

            cryptoProvider.Key = keyBytes;
            cryptoProvider.IV = iv;

            using (ICryptoTransform decryptor = cryptoProvider.CreateDecryptor())
            {
                byte[] encryptedBytes = new byte[encryptedData.Length - ivSize];
                Array.Copy(encryptedData, ivSize, encryptedBytes, 0, encryptedBytes.Length);

                byte[] plainBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                return Encoding.UTF8.GetString(plainBytes);
            }
        }
    }
}