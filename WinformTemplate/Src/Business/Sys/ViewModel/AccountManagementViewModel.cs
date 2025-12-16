using System.Collections.ObjectModel;
using System.Windows.Input;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.Service;
using WinformTemplate.Common.MVVM;
using WinformTemplate.Common.MVVM.Command;
using WinformTemplate.Logger;

namespace WinformTemplate.Business.Sys.ViewModel;

/// <summary>
/// 账户管理视图模型
/// </summary>
public class AccountManagementViewModel : BaseViewModel
{
    private readonly ISysAccountService _accountService;
    private readonly ISysRoleService _roleService;
    private ObservableCollection<SysAccountModel> _accounts = new();
    private ObservableCollection<SysRoleModel> _roles = new();
    private SysAccountModel? _selectedAccount;
    private string _searchKeyword = string.Empty;

    // 账户编辑字段
    private long _editAccountId;
    private string _editUsername = string.Empty;
    private string _editPassword = string.Empty;
    private string _editNickname = string.Empty;
    private long? _editRoleId;
    private bool _editStatus = true;
    private string _editRemark = string.Empty;

    /// <summary>
    /// 账户列表
    /// </summary>
    public ObservableCollection<SysAccountModel> Accounts
    {
        get => _accounts;
        private set => SetProperty(ref _accounts, value);
    }

    /// <summary>
    /// 角色列表
    /// </summary>
    public ObservableCollection<SysRoleModel> Roles
    {
        get => _roles;
        private set => SetProperty(ref _roles, value);
    }

    /// <summary>
    /// 选中的账户
    /// </summary>
    public SysAccountModel? SelectedAccount
    {
        get => _selectedAccount;
        set
        {
            if (SetProperty(ref _selectedAccount, value))
            {
                LoadAccountForEdit();
            }
        }
    }

    /// <summary>
    /// 搜索关键字
    /// </summary>
    public string SearchKeyword
    {
        get => _searchKeyword;
        set => SetProperty(ref _searchKeyword, value);
    }

    #region Edit Properties

    public long EditAccountId
    {
        get => _editAccountId;
        set => SetProperty(ref _editAccountId, value);
    }

    public string EditUsername
    {
        get => _editUsername;
        set => SetProperty(ref _editUsername, value);
    }

    public string EditPassword
    {
        get => _editPassword;
        set => SetProperty(ref _editPassword, value);
    }

    public string EditNickname
    {
        get => _editNickname;
        set => SetProperty(ref _editNickname, value);
    }

    public long? EditRoleId
    {
        get => _editRoleId;
        set => SetProperty(ref _editRoleId, value);
    }

    public bool EditStatus
    {
        get => _editStatus;
        set => SetProperty(ref _editStatus, value);
    }

    public string EditRemark
    {
        get => _editRemark;
        set => SetProperty(ref _editRemark, value);
    }

    #endregion

    /// <summary>
    /// 加载账户列表命令
    /// </summary>
    public ICommand LoadAccountsCommand { get; }

    /// <summary>
    /// 加载角色列表命令
    /// </summary>
    public ICommand LoadRolesCommand { get; }

    /// <summary>
    /// 搜索账户命令
    /// </summary>
    public ICommand SearchCommand { get; }

    /// <summary>
    /// 添加账户命令
    /// </summary>
    public ICommand AddAccountCommand { get; }

    /// <summary>
    /// 更新账户命令
    /// </summary>
    public ICommand UpdateAccountCommand { get; }

    /// <summary>
    /// 删除账户命令
    /// </summary>
    public ICommand DeleteAccountCommand { get; }

    /// <summary>
    /// 冻结/解冻账户命令
    /// </summary>
    public ICommand ToggleFreezeCommand { get; }

    /// <summary>
    /// 重置密码命令
    /// </summary>
    public ICommand ResetPasswordCommand { get; }

    /// <summary>
    /// 清空表单命令
    /// </summary>
    public ICommand ClearFormCommand { get; }

    /// <summary>
    /// 账户操作成功事件
    /// </summary>
    public event EventHandler<string>? OperationSucceeded;

    /// <summary>
    /// 账户操作失败事件
    /// </summary>
    public event EventHandler<string>? OperationFailed;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AccountManagementViewModel(ISysAccountService accountService, ISysRoleService roleService)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));

        LoadAccountsCommand = new RelayCommand(ExecuteLoadAccounts);
        LoadRolesCommand = new RelayCommand(ExecuteLoadRoles);
        SearchCommand = new RelayCommand(ExecuteSearch);
        AddAccountCommand = new RelayCommand(ExecuteAddAccount, CanExecuteAddAccount);
        UpdateAccountCommand = new RelayCommand(ExecuteUpdateAccount, CanExecuteUpdateAccount);
        DeleteAccountCommand = new RelayCommand(ExecuteDeleteAccount, CanExecuteDelete);
        ToggleFreezeCommand = new RelayCommand(ExecuteToggleFreeze, CanExecuteDelete);
        ResetPasswordCommand = new RelayCommand(ExecuteResetPassword, CanExecuteDelete);
        ClearFormCommand = new RelayCommand(ExecuteClearForm);
    }

    /// <summary>
    /// 加载账户列表
    /// </summary>
    private async void ExecuteLoadAccounts()
    {
        await ExecuteAsync(async () =>
        {
            Debug.Info("开始加载账户列表");

            var accounts = await _accountService.GetAllAccountsAsync();

            Accounts.Clear();
            foreach (var account in accounts)
            {
                Accounts.Add(account);
            }

            Debug.Info($"账户列表加载完成: Count={Accounts.Count}");
        }, "加载账户中...");
    }

    /// <summary>
    /// 加载角色列表
    /// </summary>
    private async void ExecuteLoadRoles()
    {
        await ExecuteAsync(async () =>
        {
            Debug.Info("开始加载角色列表");

            var roles = await _roleService.GetAllRolesAsync();

            Roles.Clear();
            foreach (var role in roles)
            {
                Roles.Add(role);
            }

            Debug.Info($"角色列表加载完成: Count={Roles.Count}");
        }, "加载角色中...");
    }

    /// <summary>
    /// 搜索账户
    /// </summary>
    private async void ExecuteSearch()
    {
        if (string.IsNullOrWhiteSpace(SearchKeyword))
        {
            ExecuteLoadAccounts();
            return;
        }

        await ExecuteAsync(async () =>
        {
            Debug.Info($"搜索账户: Keyword={SearchKeyword}");

            var accounts = await _accountService.SearchAccountsAsync(SearchKeyword);

            Accounts.Clear();
            foreach (var account in accounts)
            {
                Accounts.Add(account);
            }

            Debug.Info($"搜索完成: Count={Accounts.Count}");
        }, "搜索中...");
    }

    /// <summary>
    /// 是否可以添加账户
    /// </summary>
    private bool CanExecuteAddAccount()
    {
        return !string.IsNullOrWhiteSpace(EditUsername)
               && !string.IsNullOrWhiteSpace(EditPassword)
               && !IsBusy;
    }

    /// <summary>
    /// 添加账户
    /// </summary>
    private async void ExecuteAddAccount()
    {
        await ExecuteAsync(async () =>
        {
            Debug.Info($"添加账户: Username={EditUsername}");

            var account = new SysAccountModel
            {
                SysAccountName = EditUsername,
                SysPassword = EditPassword,
                SysNickname = EditNickname,
                SysRoleId = EditRoleId,
                SysStatus = EditStatus
            };

            var result = await _accountService.CreateAccountAsync(account);

            if (result)
            {
                Debug.Info($"账户添加成功");
                OperationSucceeded?.Invoke(this, "账户添加成功");

                ClearForm();
                ExecuteLoadAccounts();
            }
            else
            {
                Debug.Warn($"账户添加失败: Username={EditUsername}");
                OperationFailed?.Invoke(this, "账户添加失败");
            }
        }, "添加账户中...");
    }

    /// <summary>
    /// 是否可以更新账户
    /// </summary>
    private bool CanExecuteUpdateAccount()
    {
        return EditAccountId > 0
               && !string.IsNullOrWhiteSpace(EditUsername)
               && !IsBusy;
    }

    /// <summary>
    /// 更新账户
    /// </summary>
    private async void ExecuteUpdateAccount()
    {
        await ExecuteAsync(async () =>
        {
            Debug.Info($"更新账户: AccountId={EditAccountId}");

            var account = new SysAccountModel
            {
                SysId = EditAccountId,
                SysAccountName = EditUsername,
                SysNickname = EditNickname,
                SysRoleId = EditRoleId,
                SysStatus = EditStatus
            };

            // 如果密码不为空，则更新密码
            if (!string.IsNullOrWhiteSpace(EditPassword))
            {
                account.SysPassword = EditPassword;
            }

            var result = await _accountService.UpdateAccountAsync(account);

            if (result)
            {
                Debug.Info($"账户更新成功: AccountId={EditAccountId}");
                OperationSucceeded?.Invoke(this, "账户更新成功");

                ClearForm();
                ExecuteLoadAccounts();
            }
            else
            {
                Debug.Warn($"账户更新失败: AccountId={EditAccountId}");
                OperationFailed?.Invoke(this, "账户更新失败");
            }
        }, "更新账户中...");
    }

    /// <summary>
    /// 是否可以删除
    /// </summary>
    private bool CanExecuteDelete()
    {
        return SelectedAccount != null && !IsBusy;
    }

    /// <summary>
    /// 删除账户
    /// </summary>
    private async void ExecuteDeleteAccount()
    {
        if (SelectedAccount == null) return;

        await ExecuteAsync(async () =>
        {
            Debug.Info($"删除账户: AccountId={SelectedAccount.SysId}");

            var result = await _accountService.DeleteAccountAsync(SelectedAccount.SysId);

            if (result)
            {
                Debug.Info($"账户删除成功: AccountId={SelectedAccount.SysId}");
                OperationSucceeded?.Invoke(this, "账户删除成功");

                ClearForm();
                ExecuteLoadAccounts();
            }
            else
            {
                Debug.Warn($"账户删除失败: AccountId={SelectedAccount.SysId}");
                OperationFailed?.Invoke(this, "账户删除失败");
            }
        }, "删除账户中...");
    }

    /// <summary>
    /// 冻结/解冻账户
    /// </summary>
    private async void ExecuteToggleFreeze()
    {
        if (SelectedAccount == null) return;

        await ExecuteAsync(async () =>
        {
            var isFrozen = !(SelectedAccount.SysStatus ?? false);
            Debug.Info($"切换账户状态: AccountId={SelectedAccount.SysId}, IsFrozen={isFrozen}");

            bool result;
            if (isFrozen)
            {
                result = await _accountService.FreezeAccountAsync((int)SelectedAccount.SysId);
            }
            else
            {
                result = await _accountService.UnfreezeAccountAsync((int)SelectedAccount.SysId);
            }

            if (result)
            {
                var message = isFrozen ? "账户已冻结" : "账户已解冻";
                Debug.Info($"{message}: AccountId={SelectedAccount.SysId}");
                OperationSucceeded?.Invoke(this, message);

                ExecuteLoadAccounts();
            }
            else
            {
                Debug.Warn($"切换账户状态失败: AccountId={SelectedAccount.SysId}");
                OperationFailed?.Invoke(this, "操作失败");
            }
        }, "处理中...");
    }

    /// <summary>
    /// 重置密码
    /// </summary>
    private async void ExecuteResetPassword()
    {
        if (SelectedAccount == null) return;

        await ExecuteAsync(async () =>
        {
            Debug.Info($"重置密码: AccountId={SelectedAccount.SysId}");

            // 创建一个账户副本用于更新密码
            var account = new SysAccountModel
            {
                SysId = SelectedAccount.SysId,
                SysAccountName = SelectedAccount.SysAccountName,
                SysPassword = "123456"
            };

            var result = await _accountService.UpdateAccountAsync(account);

            if (result)
            {
                Debug.Info($"密码重置成功: AccountId={SelectedAccount.SysId}");
                OperationSucceeded?.Invoke(this, "密码已重置为: 123456");
            }
            else
            {
                Debug.Warn($"密码重置失败: AccountId={SelectedAccount.SysId}");
                OperationFailed?.Invoke(this, "密码重置失败");
            }
        }, "重置密码中...");
    }

    /// <summary>
    /// 清空表单
    /// </summary>
    private void ExecuteClearForm()
    {
        ClearForm();
    }

    /// <summary>
    /// 加载账户到编辑表单
    /// </summary>
    private void LoadAccountForEdit()
    {
        if (SelectedAccount == null)
        {
            ClearForm();
            return;
        }

        EditAccountId = SelectedAccount.SysId;
        EditUsername = SelectedAccount.SysAccountName ?? string.Empty;
        EditPassword = string.Empty; // 不显示密码
        EditNickname = SelectedAccount.SysNickname ?? string.Empty;
        EditRoleId = SelectedAccount.SysRoleId;
        EditStatus = SelectedAccount.SysStatus ?? true;
        EditRemark = string.Empty;
    }

    /// <summary>
    /// 清空表单
    /// </summary>
    private void ClearForm()
    {
        EditAccountId = 0;
        EditUsername = string.Empty;
        EditPassword = string.Empty;
        EditNickname = string.Empty;
        EditRoleId = null;
        EditStatus = true;
        EditRemark = string.Empty;
        SelectedAccount = null;
    }

    /// <summary>
    /// 初始化加载
    /// </summary>
    public new void Initialize()
    {
        LoadRolesCommand.Execute(null);
        LoadAccountsCommand.Execute(null);
    }
}
