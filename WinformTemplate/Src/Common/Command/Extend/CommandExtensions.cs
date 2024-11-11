using System.Diagnostics;

namespace WinformTemplate.Common.Command;

public static class CommandExtensions
{
    /// <summary>
    /// 计算 ICommand 执行 Execute 方法的间隔时间
    /// </summary>
    /// <typeparam name="T">命令类型</typeparam>
    /// <param name="command">ICommand 实例</param>
    /// <param name="input">传递给 ICommand 的 Execute 的参数</param>
    /// <param name="result">计算结果</param>
    /// <returns>时间间隔</returns>
    public static TimeSpan ExecuteAndGetExpendTimeSpan<T>(this ICommand<T> command, object? input, out T result)
    {
        var stopwatch = Stopwatch.StartNew();
        result = command.Execute(input);
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    /// <summary>
    /// 计算 ICommand 执行 Execute 方法的内存消耗。
    /// </summary>
    /// <typeparam name="T">Execute 方法的返回类型</typeparam>
    /// <param name="command">ICommand 实例</param>
    /// <param name="input">传递给 Execute 方法的输入参数</param>
    /// <param name="result">Execute 方法的执行结果</param>
    /// <returns>Execute 方法的内存消耗（以字节为单位）</returns>
    public static long ExecuteAndGetExpendMemoryUsage<T>(this ICommand<T> command, object? input, out T result)
    {
        // 执行垃圾回收，确保测试准确
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // 获取初始内存
        long beforeMemory = GC.GetTotalMemory(true);

        // 执行命令
        result = command.Execute(input);

        // 获取结束时内存
        long afterMemory = GC.GetTotalMemory(true);

        // 计算内存消耗
        return afterMemory - beforeMemory;
    }
}