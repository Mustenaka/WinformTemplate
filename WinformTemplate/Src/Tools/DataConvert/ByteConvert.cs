using System.Text;

namespace WinformTemplate.Tools.DataConvert;

public static class ByteConvert
{
    public static string ByteToString(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    public static byte[] StringToByte(string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }
}