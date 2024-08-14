namespace WinformTemplate.Common.Command;

/// <summary>
/// Behavior 行为接口 - 命令模式
/// </summary>
public interface ICommand<T>
{
    T Execute(object? input);
}