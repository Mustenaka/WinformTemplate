namespace WinformTemplate.Common.MVVM;

/// <summary>
/// ViewModel
///     视图模型 层基类，ViewModel可以快速通过OnPropertyChanged实现触发响应
/// </summary>
public class BaseViewModel : ObservableObject, IDisposable
{
    private bool _isBusy;
    private string _statusMessage;
    private bool _isDisposed;

    /// <summary>
    /// 指示ViewModel当前是否正在处理操作
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// 状态消息
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// 初始化ViewModel
    /// </summary>
    public virtual void Initialize()
    {
        // 基类中为空，由子类实现
    }

    /// <summary>
    /// 异步初始化ViewModel
    /// </summary>
    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 保护方法，用于执行带有忙状态指示的操作
    /// </summary>
    /// <param name="action">要执行的操作</param>
    /// <param name="errorHandler">错误处理器</param>
    protected async Task ExecuteAsync(Func<Task> action, Action<Exception> errorHandler = null)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await action();
        }
        catch (Exception ex)
        {
            errorHandler?.Invoke(ex);
            StatusMessage = $"错误: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// 保护方法，用于执行带有忙状态指示和返回值的操作
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="action">要执行的操作</param>
    /// <param name="errorHandler">错误处理器</param>
    /// <returns>操作结果</returns>
    protected async Task<T> ExecuteAsync<T>(Func<Task<T>> action, Action<Exception> errorHandler = null)
    {
        if (IsBusy)
            return default;

        try
        {
            IsBusy = true;
            return await action();
        }
        catch (Exception ex)
        {
            errorHandler?.Invoke(ex);
            StatusMessage = $"错误: {ex.Message}";
            return default;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing">是否正在释放托管资源</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            // 释放托管资源
        }

        _isDisposed = true;
    }
}