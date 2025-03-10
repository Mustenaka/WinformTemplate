namespace WinformTemplate.Common.MVVM.Command.Interface;

public interface IRelayCommand
{
    /// <summary>
    /// 判断命令是否可以执行
    /// </summary>
    /// <returns>是否可以执行</returns>
    bool CanExecute();

    /// <summary>
    /// 执行命令
    /// </summary>
    void Execute();
}