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
    /// <param name="message">日志消息</param>
    public static void Info(string message)
    {
        if (Debug.IsStreamOnConsole)
        {
            Console.WriteLine($"[INFO] {message}");
        }

        if (logInfo.IsInfoEnabled)
        {
            logInfo.Info(message);
        }
    }

    /// <summary>
    /// Info（信息）级别日志，带格式化参数
    /// </summary>
    /// <param name="format">格式化字符串</param>
    /// <param name="args">参数</param>
    public static void Info(string format, params object[] args)
    {
        Info(string.Format(format, args));
    }

    /// <summary>
    /// Warn（警告）级别日志
    /// </summary>
    /// <param name="message">日志消息</param>
    public static void Warn(string message)
    {
        if (Debug.IsStreamOnConsole)
        {
            Console.WriteLine($"[WARN] {message}");
        }

        if (logWarn.IsWarnEnabled)
        {
            logWarn.Warn(message);
        }
    }

    /// <summary>
    /// Warn（警告）级别日志，带格式化参数
    /// </summary>
    /// <param name="format">格式化字符串</param>
    /// <param name="args">参数</param>
    public static void Warn(string format, params object[] args)
    {
        Warn(string.Format(format, args));
    }

    /// <summary>
    /// Error（错误）级别日志
    /// </summary>
    /// <param name="message">日志消息</param>
    public static void Error(string message)
    {
        if (Debug.IsStreamOnConsole)
        {
            Console.WriteLine($"[ERROR] {message}");
        }

        if (logError.IsErrorEnabled)
        {
            logError.Error(message);
        }
    }

    /// <summary>
    /// Error（错误）级别日志，带异常
    /// </summary>
    /// <param name="message">日志消息</param>
    /// <param name="exception">异常对象</param>
    public static void Error(string message, Exception exception)
    {
        var fullMessage = $"{message}\nException: {exception.GetType().Name}\nMessage: {exception.Message}\nStackTrace: {exception.StackTrace}";

        if (Debug.IsStreamOnConsole)
        {
            Console.WriteLine($"[ERROR] {fullMessage}");
        }

        if (logError.IsErrorEnabled)
        {
            logError.Error(message, exception);
        }
    }

    /// <summary>
    /// Error（错误）级别日志，带格式化参数
    /// </summary>
    /// <param name="format">格式化字符串</param>
    /// <param name="args">参数</param>
    public static void Error(string format, params object[] args)
    {
        Error(string.Format(format, args));
    }

    /// <summary>
    /// Fatal（致命错误）级别日志
    /// </summary>
    /// <param name="message">日志消息</param>
    public static void Fatal(string message)
    {
        if (Debug.IsStreamOnConsole)
        {
            Console.WriteLine($"[FATAL] {message}");
        }

        if (logFatal.IsFatalEnabled)
        {
            logFatal.Fatal(message);
        }
    }

    /// <summary>
    /// Fatal（致命错误）级别日志，带异常
    /// </summary>
    /// <param name="message">日志消息</param>
    /// <param name="exception">异常对象</param>
    public static void Fatal(string message, Exception exception)
    {
        var fullMessage = $"{message}\nException: {exception.GetType().Name}\nMessage: {exception.Message}\nStackTrace: {exception.StackTrace}";

        if (Debug.IsStreamOnConsole)
        {
            Console.WriteLine($"[FATAL] {fullMessage}");
        }

        if (logFatal.IsFatalEnabled)
        {
            logFatal.Fatal(message, exception);
        }
    }

    /// <summary>
    /// Fatal（致命错误）级别日志，带格式化参数
    /// </summary>
    /// <param name="format">格式化字符串</param>
    /// <param name="args">参数</param>
    public static void Fatal(string format, params object[] args)
    {
        Fatal(string.Format(format, args));
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