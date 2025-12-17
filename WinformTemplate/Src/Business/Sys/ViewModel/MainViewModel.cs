using System.Collections.ObjectModel;
using System.Windows.Input;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.Service;
using WinformTemplate.Common.MVVM;
using WinformTemplate.Common.MVVM.Command;
using WinformTemplate.Logger;

namespace WinformTemplate.Business.Sys.ViewModel;

/// <summary>
/// 主界面视图模型
/// </summary>
public class MainViewModel : BaseViewModel
{
    private readonly IPermissionService _permissionService;
    private readonly ISysMenuService _menuService;
    private SysAccountModel? _currentAccount;
    private ObservableCollection<SysMenuModel> _menuItems = new();
    private SysMenuModel? _selectedMenu;

    /// <summary>
    /// 当前登录账户
    /// </summary>
    public SysAccountModel? CurrentAccount
    {
        get => _currentAccount;
        set
        {
            if (SetProperty(ref _currentAccount, value))
            {
                // 账户变化时重新加载菜单
                LoadMenusCommand.Execute(null);
            }
        }
    }

    /// <summary>
    /// 菜单项集合
    /// </summary>
    public ObservableCollection<SysMenuModel> MenuItems
    {
        get => _menuItems;
        private set => SetProperty(ref _menuItems, value);
    }

    /// <summary>
    /// 当前选中的菜单
    /// </summary>
    public SysMenuModel? SelectedMenu
    {
        get => _selectedMenu;
        set => SetProperty(ref _selectedMenu, value);
    }

    /// <summary>
    /// 加载菜单命令
    /// </summary>
    public ICommand LoadMenusCommand { get; }

    /// <summary>
    /// 退出登录命令
    /// </summary>
    public ICommand LogoutCommand { get; }

    /// <summary>
    /// 菜单选择变化事件
    /// </summary>
    public event EventHandler<SysMenuModel>? MenuSelected;

    /// <summary>
    /// 退出登录事件
    /// </summary>
    public event EventHandler? LogoutRequested;

    /// <summary>
    /// 构造函数
    /// </summary>
    public MainViewModel(IPermissionService permissionService, ISysMenuService menuService)
    {
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));

        LoadMenusCommand = new RelayCommand(ExecuteLoadMenus, CanExecuteLoadMenus);
        LogoutCommand = new RelayCommand(ExecuteLogout);
    }

    /// <summary>
    /// 是否可以加载菜单
    /// </summary>
    private bool CanExecuteLoadMenus()
    {
        return CurrentAccount != null && !IsBusy;
    }

    /// <summary>
    /// 执行加载菜单
    /// </summary>
    private async void ExecuteLoadMenus()
    {
        if (CurrentAccount == null)
        {
            Debug.Warn("尝试加载菜单但当前账户为空");
            return;
        }

        await ExecuteAsync(async () =>
        {
            Debug.Info($"开始加载用户菜单: AccountId={CurrentAccount.SysId}");

            // 获取用户可访问的菜单树
            var menus = await _permissionService.GetAccessibleMenuTreeAsync(CurrentAccount.SysId);

            // 更新菜单集合
            MenuItems.Clear();
            foreach (var menu in menus.OrderBy(m => m.SmSort))
            {
                MenuItems.Add(menu);
            }

            Debug.Info($"菜单加载完成: Count={MenuItems.Count}");
        }, ex =>
        {
            Debug.Error($"加载菜单异常: {ex.Message}", ex);
        });
    }

    /// <summary>
    /// 执行退出登录
    /// </summary>
    private void ExecuteLogout()
    {
        Debug.Info($"用户退出登录: Username={CurrentAccount?.SysAccountName}");

        CurrentAccount = null;
        MenuItems.Clear();
        SelectedMenu = null;

        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 选择菜单项
    /// </summary>
    public void SelectMenu(SysMenuModel menu)
    {
        if (menu == null) return;

        SelectedMenu = menu;
        Debug.Info($"选择菜单: MenuName={menu.SmName}, MenuId={menu.SmId}");

        MenuSelected?.Invoke(this, menu);
    }

    /// <summary>
    /// 检查是否有权限访问指定菜单
    /// </summary>
    public async Task<bool> HasPermissionAsync(long menuId)
    {
        if (CurrentAccount == null)
        {
            Debug.Warn("检查权限失败: 当前账户为空");
            return false;
        }

        return await _permissionService.HasPermissionAsync(CurrentAccount.SysId, menuId);
    }

    /// <summary>
    /// 刷新菜单
    /// </summary>
    public void RefreshMenus()
    {
        if (LoadMenusCommand.CanExecute(null))
        {
            LoadMenusCommand.Execute(null);
        }
    }
}
