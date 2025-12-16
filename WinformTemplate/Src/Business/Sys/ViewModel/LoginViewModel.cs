using System.Windows.Input;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.Service;
using WinformTemplate.Common.MVVM;
using WinformTemplate.Common.MVVM.Command;
using WinformTemplate.Logger;

namespace WinformTemplate.Business.Sys.ViewModel;

/// <summary>
/// 登录视图模型
/// </summary>
public class LoginViewModel : BaseViewModel
{
    private readonly ISysAccountService _accountService;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private SysAccountModel? _currentAccount;

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                ErrorMessage = string.Empty;
                (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                ErrorMessage = string.Empty;
                (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>
    /// 当前登录的账户
    /// </summary>
    public SysAccountModel? CurrentAccount
    {
        get => _currentAccount;
        private set => SetProperty(ref _currentAccount, value);
    }

    /// <summary>
    /// 登录命令
    /// </summary>
    public ICommand LoginCommand { get; }

    /// <summary>
    /// 登录成功事件
    /// </summary>
    public event EventHandler<SysAccountModel>? LoginSucceeded;

    /// <summary>
    /// 构造函数
    /// </summary>
    public LoginViewModel(ISysAccountService accountService)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
    }

    /// <summary>
    /// 是否可以执行登录
    /// </summary>
    private bool CanExecuteLogin()
    {
        return !string.IsNullOrWhiteSpace(Username)
               && !string.IsNullOrWhiteSpace(Password)
               && !IsBusy;
    }

    /// <summary>
    /// 执行登录
    /// </summary>
    private async void ExecuteLogin()
    {
        await ExecuteAsync(async () =>
        {
            Debug.Info($"开始登录: Username={Username}");

            // 调用登录服务
            var account = await _accountService.LoginAsync(Username, Password);

            if (account == null)
            {
                ErrorMessage = "用户名或密码错误，请重试";
                Debug.Warn($"登录失败: Username={Username}");
                return;
            }

            // 登录成功
            CurrentAccount = account;
            ErrorMessage = string.Empty;

            Debug.Info($"登录成功: Username={Username}, AccountId={account.SysId}, AccountName={account.SysAccountName}");

            // 触发登录成功事件
            LoginSucceeded?.Invoke(this, account);
        }, "登录中...");
    }

    /// <summary>
    /// 清空表单
    /// </summary>
    public void ClearForm()
    {
        Username = string.Empty;
        Password = string.Empty;
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// 记住用户名（用于自动填充）
    /// </summary>
    public void RememberUsername(string username)
    {
        Username = username;
    }
}
