using System.Security.Cryptography;
using System.Text;

namespace WinformTemplate.Tools.Encryption.Unsymmetric.RSAHelper;

public class RSAHelper
{
    /// <summary>
    /// 生成公钥和私钥
    /// </summary>
    /// <param name="keySize">密钥大小</param>
    /// <param name="privateKey">输出私钥</param>
    /// <param name="publicKey">输出公钥</param>
    public static void GenerateKeys(int keySize, out string privateKey, out string publicKey)
    {
        using (var rsa = new RSACryptoServiceProvider(keySize))
        {
            privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
            publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
        }
    }

    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="publicKey">公钥</param>
    /// <param name="data">要加密的数据</param>
    /// <returns>加密后的数据</returns>
    public static string Encrypt(string publicKey, string data)
    {
        using (var rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
            var bytes = Encoding.UTF8.GetBytes(data);
            var encryptedBytes = rsa.Encrypt(bytes, RSAEncryptionPadding.Pkcs1);
            return Convert.ToBase64String(encryptedBytes);
        }
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="privateKey">私钥</param>
    /// <param name="data">要解密的数据</param>
    /// <returns>解密后的数据</returns>
    public static string Decrypt(string privateKey, string data)
    {
        using (var rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
            var encryptedBytes = Convert.FromBase64String(data);
            var decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }

    /// <summary>
    /// RSA私钥签名
    /// </summary>
    /// <param name="privateKey">私钥</param>
    /// <param name="data">要签名的数据</param>
    /// <returns>签名数据</returns>
    public static string SignData(string privateKey, string data)
    {
        using (var rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
            var bytes = Encoding.UTF8.GetBytes(data);
            var signatureBytes = rsa.SignData(bytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signatureBytes);
        }
    }

    /// <summary>
    /// 验证RSA签名
    /// </summary>
    /// <param name="publicKey">公钥</param>
    /// <param name="data">原始数据</param>
    /// <param name="signatureData">签名数据</param>
    /// <returns></returns>
    public static bool VerifyData(string publicKey, string data, string signatureData)
    {
        using (var rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
            var bytes = Encoding.UTF8.GetBytes(data);
            var signatureBytes = Convert.FromBase64String(signatureData);
            return rsa.VerifyData(bytes, signatureBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        }
    }
}