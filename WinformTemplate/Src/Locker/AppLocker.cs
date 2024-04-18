using WinformTemplate.Tools.DataConvert;
using WinformTemplate.Tools.Encryption.Symmetric.DESHelper;

namespace WinformTemplate.Locker;

/// <summary>
/// 本模块用于应用使用周期保证，编译时安装一个内置时间处理脚本，
/// 一旦超时则无法使用应用，并同时允许重新激活
/// </summary>
public class AppLocker
{
    public string Key = "3K7FJ9W4N8X5Y2HZ";
    public DateTime currentTime;
    public DateTime lockTime;

    /// <summary>
    /// 使用上锁时间模块
    /// </summary>
    /// <param name="lockTime">上锁时间</param>
    public AppLocker(DateTime lockTime)
    {
        this.lockTime = lockTime;
    }

    /// <summary>
    /// 使用上锁时间模块
    /// </summary>
    /// <param name="encryptionData">加密数据</param>
    public AppLocker(string encryptionData)
    {
        this.lockTime = GetLockTime(encryptionData, Key);
    }

    /// <summary>
    /// 创建应用使用周期时间
    /// </summary>
    /// <param name="years">年</param>
    /// <param name="months">月</param>
    /// <param name="days">日</param>
    /// <param name="hours">小时</param>
    /// <param name="minutes">分钟</param>
    /// <param name="second">秒</param>
    public AppLocker(int years = 0, int months = 0, int days = 0, int hours = 0, int minutes = 0, int second = 0)
    {
        currentTime = DateTime.Now;
        lockTime = DateTime.Now;

        lockTime = lockTime.AddYears(years);
        lockTime = lockTime.AddMonths(months);
        lockTime = lockTime.AddDays(days);
        lockTime = lockTime.AddHours(hours);
        lockTime = lockTime.AddMinutes(minutes);
        lockTime = lockTime.AddSeconds(second);
    }

    /// <summary>
    /// 检查当前App是否在生效时间内
    /// </summary>
    /// <returns></returns>
    public bool CheckAppAlive()
    {
        // 检查当前时间和上锁时间
        if (currentTime < lockTime)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 通过存储数据和解密key生成时间序
    /// </summary>
    /// <param name="data">存储的加密数据</param>
    /// <param name="key">解密key</param>
    /// <returns>对应的存储时间，该数据可以交给lockTime</returns>
    public DateTime GetLockTime(string data, string key)
    {
        var bytes = ByteConvert.StringToByte(data);
        string DecryptionStr = DESHelper.DecryptDES(bytes, key);

        DateTime time = DateTime.Parse(DecryptionStr);
        return time;
    }
}