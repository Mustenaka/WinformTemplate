using System.Collections.ObjectModel;
using System.Windows.Input;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.Service;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Common.MVVM;
using WinformTemplate.Common.MVVM.Command;
using WinformTemplate.Logger;

namespace WinformTemplate.Business.Sys.ViewModel;

/// <summary>
/// 账户管理视图模型。
/// </summary>
public class AccountManagementViewModel : BaseViewModel
{
    private readonly ISysAccountService _accountService;
    private readonly ISysRoleService _roleService;
    private ObservableCollection<SysAccountModel> _accounts = new();
    private ObservableCollection<SysRoleModel> _roles = new();
    private SysAccountModel? _selectedAccount;
    private string _searchKeyword = string.Empty;
    private int _currentPage = 1;
    private int _pageSize = 20;
    private int _totalCount;

    private long _editAccountId;
    private string _editUsername = string.Empty;
    private string _editPassword = string.Empty;
    private string _editNickname = string.Empty;
    private long? _editRoleId;
    private bool _editStatus = true;
    private string _editRemark = string.Empty;

    public AccountManagementViewModel(ISysAccountService accountService, ISysRoleService roleService)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));

        LoadAccountsCommand = new RelayCommand(ExecuteLoadAccounts);
        LoadRolesCommand = new RelayCommand(ExecuteLoadRoles);
        SearchCommand = new RelayCommand(ExecuteSearch);
        PreviousPageCommand = new RelayCommand(ExecutePreviousPage);
        NextPageCommand = new RelayCommand(ExecuteNextPage);
        AddAccountCommand = new RelayCommand(ExecuteAddAccount, CanExecuteAddAccount);
        UpdateAccountCommand = new RelayCommand(ExecuteUpdateAccount, CanExecuteUpdateAccount);
        DeleteAccountCommand = new RelayCommand(ExecuteDeleteAccount, CanExecuteDelete);
        ToggleFreezeCommand = new RelayCommand(ExecuteToggleFreeze, CanExecuteDelete);
        ResetPasswordCommand = new RelayCommand(ExecuteResetPassword, CanExecuteDelete);
        ClearFormCommand = new RelayCommand(ExecuteClearForm);
    }

    public ObservableCollection<SysAccountModel> Accounts
    {
        get => _accounts;
        private set => SetProperty(ref _accounts, value);
    }

    public ObservableCollection<SysRoleModel> Roles
    {
        get => _roles;
        private set => SetProperty(ref _roles, value);
    }

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

    public string SearchKeyword
    {
        get => _searchKeyword;
        set => SetProperty(ref _searchKeyword, value);
    }

    public int CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (SetProperty(ref _currentPage, Math.Max(1, value)))
            {
                RaisePagingPropertiesChanged();
            }
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            var normalized = Math.Clamp(value, 1, 200);
            if (SetProperty(ref _pageSize, normalized))
            {
                RaisePagingPropertiesChanged();
            }
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        private set
        {
            if (SetProperty(ref _totalCount, Math.Max(0, value)))
            {
                RaisePagingPropertiesChanged();
            }
        }
    }

    public int TotalPages => CalculateTotalPages(TotalCount, PageSize);

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;

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

    public ICommand LoadAccountsCommand { get; }

    public ICommand LoadRolesCommand { get; }

    public ICommand SearchCommand { get; }

    public ICommand PreviousPageCommand { get; }

    public ICommand NextPageCommand { get; }

    public ICommand AddAccountCommand { get; }

    public ICommand UpdateAccountCommand { get; }

    public ICommand DeleteAccountCommand { get; }

    public ICommand ToggleFreezeCommand { get; }

    public ICommand ResetPasswordCommand { get; }

    public ICommand ClearFormCommand { get; }

    public event EventHandler<string>? OperationSucceeded;

    public event EventHandler<string>? OperationFailed;

    public Task LoadAccountsAsync()
    {
        return ExecuteAsync(LoadAccountsPageAsync, ex => Debug.Error($"加载账户异常: {ex.Message}", ex));
    }

    public Task LoadRolesAsync()
    {
        return ExecuteAsync(async () =>
        {
            Debug.Info("开始加载角色列表");

            var roles = await _roleService.GetAllRolesAsync();

            Roles.Clear();
            foreach (var role in roles)
            {
                Roles.Add(role);
            }

            Debug.Info($"角色列表加载完成: Count={Roles.Count}");
        }, ex => Debug.Error($"加载角色异常: {ex.Message}", ex));
    }

    public Task SearchAsync()
    {
        CurrentPage = 1;
        return LoadAccountsAsync();
    }

    public Task GoToPageAsync(int page)
    {
        CurrentPage = Math.Clamp(page, 1, TotalPages);
        return LoadAccountsAsync();
    }

    public Task SetPageSizeAsync(int pageSize)
    {
        PageSize = pageSize;
        CurrentPage = 1;
        return LoadAccountsAsync();
    }

    public override void Initialize()
    {
        _ = InitializeAccountPageAsync();
    }

    public override Task InitializeAsync()
    {
        return InitializeAccountPageAsync();
    }

    private async Task InitializeAccountPageAsync()
    {
        await LoadRolesAsync();
        await LoadAccountsAsync();
    }

    private async void ExecuteLoadAccounts()
    {
        await LoadAccountsAsync();
    }

    private async void ExecuteLoadRoles()
    {
        await LoadRolesAsync();
    }

    private async void ExecuteSearch()
    {
        await SearchAsync();
    }

    private async void ExecutePreviousPage()
    {
        await GoToPageAsync(CurrentPage - 1);
    }

    private async void ExecuteNextPage()
    {
        await GoToPageAsync(CurrentPage + 1);
    }

    private async Task LoadAccountsPageAsync()
    {
        Debug.Info($"加载账户: Keyword={SearchKeyword}, Page={CurrentPage}, PageSize={PageSize}");

        var result = await QueryCurrentPageAsync();
        var lastPage = CalculateTotalPages(result.Total, PageSize);
        if (CurrentPage > lastPage)
        {
            CurrentPage = lastPage;
            result = await QueryCurrentPageAsync();
        }

        Accounts.Clear();
        foreach (var account in result.Items)
        {
            Accounts.Add(account);
        }

        TotalCount = result.Total;
        StatusMessage = $"已加载 {Accounts.Count}/{TotalCount} 个账户";

        Debug.Info($"账户列表加载完成: Count={Accounts.Count}, Total={TotalCount}, Page={CurrentPage}");
    }

    private Task<PagedResult<SysAccountModel>> QueryCurrentPageAsync()
    {
        var keyword = string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword.Trim();
        return _accountService.QueryAccountsAsync(keyword, CurrentPage, PageSize);
    }

    private bool CanExecuteAddAccount()
    {
        return !string.IsNullOrWhiteSpace(EditUsername)
               && !string.IsNullOrWhiteSpace(EditPassword)
               && !IsBusy;
    }

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
                Debug.Info("账户添加成功");
                OperationSucceeded?.Invoke(this, "账户添加成功");

                ClearForm();
                await LoadAccountsPageAsync();
            }
            else
            {
                Debug.Warn($"账户添加失败: Username={EditUsername}");
                OperationFailed?.Invoke(this, "账户添加失败");
            }
        }, ex => Debug.Error($"添加账户异常: {ex.Message}", ex));
    }

    private bool CanExecuteUpdateAccount()
    {
        return EditAccountId > 0
               && !string.IsNullOrWhiteSpace(EditUsername)
               && !IsBusy;
    }

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
                await LoadAccountsPageAsync();
            }
            else
            {
                Debug.Warn($"账户更新失败: AccountId={EditAccountId}");
                OperationFailed?.Invoke(this, "账户更新失败");
            }
        }, ex => Debug.Error($"更新账户异常: {ex.Message}", ex));
    }

    private bool CanExecuteDelete()
    {
        return SelectedAccount != null && !IsBusy;
    }

    private async void ExecuteDeleteAccount()
    {
        if (SelectedAccount == null)
        {
            return;
        }

        await ExecuteAsync(async () =>
        {
            Debug.Info($"删除账户: AccountId={SelectedAccount.SysId}");

            var result = await _accountService.DeleteAccountAsync(SelectedAccount.SysId);

            if (result)
            {
                Debug.Info($"账户删除成功: AccountId={SelectedAccount.SysId}");
                OperationSucceeded?.Invoke(this, "账户删除成功");

                ClearForm();
                await LoadAccountsPageAsync();
            }
            else
            {
                Debug.Warn($"账户删除失败: AccountId={SelectedAccount.SysId}");
                OperationFailed?.Invoke(this, "账户删除失败");
            }
        }, ex => Debug.Error($"删除账户异常: {ex.Message}", ex));
    }

    private async void ExecuteToggleFreeze()
    {
        if (SelectedAccount == null)
        {
            return;
        }

        await ExecuteAsync(async () =>
        {
            var isFrozen = !(SelectedAccount.SysStatus ?? false);
            Debug.Info($"切换账户状态: AccountId={SelectedAccount.SysId}, IsFrozen={isFrozen}");

            bool result;
            if (isFrozen)
            {
                result = await _accountService.FreezeAccountAsync(SelectedAccount.SysId);
            }
            else
            {
                result = await _accountService.UnfreezeAccountAsync(SelectedAccount.SysId);
            }

            if (result)
            {
                var message = isFrozen ? "账户已冻结" : "账户已解冻";
                Debug.Info($"{message}: AccountId={SelectedAccount.SysId}");
                OperationSucceeded?.Invoke(this, message);

                await LoadAccountsPageAsync();
            }
            else
            {
                Debug.Warn($"切换账户状态失败: AccountId={SelectedAccount.SysId}");
                OperationFailed?.Invoke(this, "操作失败");
            }
        }, ex => Debug.Error($"操作异常: {ex.Message}", ex));
    }

    private async void ExecuteResetPassword()
    {
        if (SelectedAccount == null)
        {
            return;
        }

        await ExecuteAsync(async () =>
        {
            Debug.Info($"重置密码: AccountId={SelectedAccount.SysId}");

            var account = new SysAccountModel
            {
                SysId = SelectedAccount.SysId,
                SysAccountName = SelectedAccount.SysAccountName,
                SysPassword = "123456",
                SysNickname = SelectedAccount.SysNickname,
                SysLevel = SelectedAccount.SysLevel,
                SysRoleId = SelectedAccount.SysRoleId,
                SysExtendId = SelectedAccount.SysExtendId,
                SysStatus = SelectedAccount.SysStatus,
                SysReserved1 = SelectedAccount.SysReserved1,
                SysReserved2 = SelectedAccount.SysReserved2,
                SysReserved3 = SelectedAccount.SysReserved3
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
        }, ex => Debug.Error($"重置密码异常: {ex.Message}", ex));
    }

    private void ExecuteClearForm()
    {
        ClearForm();
    }

    private void LoadAccountForEdit()
    {
        if (SelectedAccount == null)
        {
            ClearForm();
            return;
        }

        EditAccountId = SelectedAccount.SysId;
        EditUsername = SelectedAccount.SysAccountName ?? string.Empty;
        EditPassword = string.Empty;
        EditNickname = SelectedAccount.SysNickname ?? string.Empty;
        EditRoleId = SelectedAccount.SysRoleId;
        EditStatus = SelectedAccount.SysStatus ?? true;
        EditRemark = string.Empty;
    }

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

    private static int CalculateTotalPages(int totalCount, int pageSize)
    {
        return Math.Max(1, (int)Math.Ceiling(Math.Max(0, totalCount) / (double)Math.Max(pageSize, 1)));
    }

    private void RaisePagingPropertiesChanged()
    {
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(HasPreviousPage));
        OnPropertyChanged(nameof(HasNextPage));
    }
}
