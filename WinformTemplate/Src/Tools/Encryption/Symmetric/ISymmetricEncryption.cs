namespace WinformTemplate.Tools.Encryption.Symmetric;

public interface ISymmetricEncryption
{
    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="plainText">加密字符串</param>
    /// <param name="key">解密Key</param>
    /// <returns>加密结果</returns>
    public static abstract byte[] EncryptDES(string plainText, string key);

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="encryptedData">加密数据</param>
    /// <param name="key">解密Key</param>
    /// <returns>解密结果</returns>
    public static abstract string DecryptDES(byte[] encryptedData, string key);
}