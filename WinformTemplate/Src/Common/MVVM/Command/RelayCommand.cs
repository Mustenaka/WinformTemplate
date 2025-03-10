using WinformTemplate.Common.MVVM.Command.Interface;

namespace WinformTemplate.Common.MVVM.Command;

/// <summary>
/// 表示一个命令
/// </summary>
public class RelayCommand : IRelayCommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="execute">执行方法</param>
    /// <param name="canExecute">判断是否可执行的方法</param>
    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// 判断命令是否可以执行
    /// </summary>
    /// <returns>是否可以执行</returns>
    public bool CanExecute()
    {
        return _canExecute?.Invoke() ?? true;
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    public void Execute()
    {
        if (CanExecute())
            _execute();
    }
}

/// <summary>
/// 表示一个带参数的命令
/// </summary>
/// <typeparam name="T">参数类型</typeparam>
public class RelayCommand<T>
{
    private readonly Action<T> _execute;
    private readonly Predicate<T> _canExecute;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="execute">执行方法</param>
    /// <param name="canExecute">判断是否可执行的方法</param>
    public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// 判断命令是否可以执行
    /// </summary>
    /// <param name="parameter">命令参数</param>
    /// <returns>是否可以执行</returns>
    public bool CanExecute(T parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="parameter">命令参数</param>
    public void Execute(T parameter)
    {
        if (CanExecute(parameter))
            _execute(parameter);
    }
}