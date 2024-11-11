using log4net.Config;

namespace WinformTemplate.Logger;

/// <summary>
/// 改写的测试输出模块
/// </summary>
public class Debug
{
    private static readonly log4net.ILog logInfo = log4net.LogManager.GetLogger("loginfo");
    private static readonly log4net.ILog logError = log4net.LogManager.GetLogger("logerror");
    private static readonly log4net.ILog logWarn = log4net.LogManager.GetLogger("loginfo");
    private static readonly log4net.ILog logFatal = log4net.LogManager.GetLogger("logerror");

    /// <summary>
    /// 是否在控制台输出
    /// </summary>
    public static bool IsStreamOnConsole = true;

    /// <summary>
    /// Info（信息）级别日志
    /// </summary>
    /// <param name="message"></param>
    public static void Info(string message)
    {
        if (Debug.IsStreamOnConsole)
        {
            Console.WriteLine(message);
        }

        if (logInfo.IsInfoEnabled)
        {
            logInfo.Info(message);
        }
    }

    /// <summary>
    /// Warn（警告）级别日志
    /// </summary>
    /// <param name="message"></param>
    public static void Warn(string message)
    {
        if (Debug.IsStreamOnConsole)
        {
            Console.WriteLine(message);
        }

        if (logWarn.IsWarnEnabled)
        {
            logWarn.Fatal(message);
        }
    }

    /// <summary>
    /// Error（错误）级别日志
    /// </summary>
    /// <param name="message"></param>
    public static void Error(string message)
    {
        if (Debug.IsStreamOnConsole)
        {
            Console.WriteLine(message);
        }

        if (logError.IsErrorEnabled)
        {
            logError.Error(message);
        }
    }

    /// <summary>
    /// Fatal（致命错误）级别日志
    /// </summary>
    /// <param name="message"></param>
    public static void Fatal(string message)
    {
        if (Debug.IsStreamOnConsole)
        {
            Console.WriteLine(message);
        }

        if (logFatal.IsFatalEnabled)
        {
            logFatal.Fatal(message);
        }
    }

    /// <summary>
    /// 初始化日志模块
    /// </summary>
    public static void InitLog4Net()
    {
        var logCfg = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "Resources/Log4net/log4net.config");
        XmlConfigurator.ConfigureAndWatch(logCfg);
    }
}