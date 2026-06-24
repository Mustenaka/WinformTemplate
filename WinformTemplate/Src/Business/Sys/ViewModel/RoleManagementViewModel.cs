using System.Collections.ObjectModel;
using System.Windows.Input;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.Service;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Common.MVVM;
using WinformTemplate.Common.MVVM.Command;
using WinformTemplate.Logger;

namespace WinformTemplate.Business.Sys.ViewModel;

public sealed class RoleManagementViewModel : BaseViewModel
{
    private readonly ISysRoleService _roleService;
    private readonly ISysMenuService _menuService;
    private readonly IPermissionService _permissionService;
    private readonly ISysAccountService _accountService;
    private readonly List<SysRoleModel> _allRoles = new();
    private readonly List<SysMenuModel> _allMenus = new();

    private ObservableCollection<SysRoleModel> _roles = new();
    private ObservableCollection<RoleMenuPermissionItem> _menuPermissions = new();
    private SysRoleModel? _selectedRole;
    private string? _searchKeyword;
    private long _editRoleId;
    private string _editName = string.Empty;
    private string _editEnName = string.Empty;
    private string _editRemark = string.Empty;
    private bool _editStatus = true;

    public RoleManagementViewModel(
        ISysRoleService roleService,
        ISysMenuService menuService,
        IPermissionService permissionService,
        ISysAccountService accountService)
    {
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));

        LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
        SearchCommand = new RelayCommand(async () => await SearchAsync());
        ResetSearchCommand = new RelayCommand(async () => await ResetSearchAsync());
        RefreshCommand = new RelayCommand(async () => await LoadDataAsync());
        SavePermissionsCommand = new RelayCommand(async () => await SavePermissionsAsync(), () => HasSelectedRole);
    }

    public ObservableCollection<SysRoleModel> Roles
    {
        get => _roles;
        private set => SetProperty(ref _roles, value);
    }

    public ObservableCollection<RoleMenuPermissionItem> MenuPermissions
    {
        get => _menuPermissions;
        private set
        {
            if (SetProperty(ref _menuPermissions, value))
            {
                OnPropertyChanged(nameof(SelectedMenuIds));
            }
        }
    }

    public SysRoleModel? SelectedRole
    {
        get => _selectedRole;
        private set
        {
            if (SetProperty(ref _selectedRole, value))
            {
                LoadRoleForEdit();
                OnPropertyChanged(nameof(HasSelectedRole));
            }
        }
    }

    public bool HasSelectedRole => SelectedRole != null;

    public string? SearchKeyword
    {
        get => _searchKeyword;
        set => SetProperty(ref _searchKeyword, value);
    }

    public long EditRoleId
    {
        get => _editRoleId;
        private set => SetProperty(ref _editRoleId, value);
    }

    public string EditName
    {
        get => _editName;
        private set => SetProperty(ref _editName, value);
    }

    public string EditEnName
    {
        get => _editEnName;
        private set => SetProperty(ref _editEnName, value);
    }

    public string EditRemark
    {
        get => _editRemark;
        private set => SetProperty(ref _editRemark, value);
    }

    public bool EditStatus
    {
        get => _editStatus;
        private set => SetProperty(ref _editStatus, value);
    }

    public IReadOnlyCollection<long> SelectedMenuIds => MenuPermissions
        .Where(item => item.IsChecked)
        .Select(item => item.MenuId)
        .Distinct()
        .ToArray();

    public ICommand LoadDataCommand { get; }

    public ICommand SearchCommand { get; }

    public ICommand ResetSearchCommand { get; }

    public ICommand RefreshCommand { get; }

    public ICommand SavePermissionsCommand { get; }

    public event EventHandler<string>? OperationCompleted;

    public override async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            await ReloadMenusCoreAsync();
            await ReloadRolesCoreAsync();
            await LoadMenuPermissionsCoreAsync();
        }, ex =>
        {
            Debug.Error("Load role management data failed", ex);
            Roles = new ObservableCollection<SysRoleModel>();
            MenuPermissions = new ObservableCollection<RoleMenuPermissionItem>();
        });
    }

    public async Task SearchAsync()
    {
        ApplyRoleFilter();
        SelectFilteredRole();
        await LoadMenuPermissionsAsync();
    }

    public async Task ResetSearchAsync()
    {
        SearchKeyword = null;
        await SearchAsync();
    }

    public async Task SelectRoleAsync(SysRoleModel? role)
    {
        SelectedRole = role == null
            ? null
            : Roles.FirstOrDefault(item => item.SrId == role.SrId) ?? _allRoles.FirstOrDefault(item => item.SrId == role.SrId);

        await LoadMenuPermissionsAsync();
    }

    public void SetMenuChecked(long menuId, bool isChecked)
    {
        var item = MenuPermissions.FirstOrDefault(menu => menu.MenuId == menuId);
        if (item == null)
        {
            return;
        }

        item.IsChecked = isChecked;
        OnPropertyChanged(nameof(SelectedMenuIds));
    }

    public async Task<(bool Success, string Message)> SaveRoleAsync(SysRoleModel role)
    {
        ArgumentNullException.ThrowIfNull(role);

        return await RunOperationAsync(async () =>
        {
            NormalizeRole(role);
            ValidateRole(role);

            var saved = role.SrId > 0
                ? await _roleService.UpdateRoleAsync(role)
                : await _roleService.CreateRoleAsync(role);

            if (!saved)
            {
                return Fail("角色保存失败：角色不存在或服务拒绝保存。");
            }

            var preferredRoleId = role.SrId > 0 ? role.SrId : (long?)null;
            await ReloadRolesCoreAsync(preferredRoleId);
            await LoadMenuPermissionsCoreAsync();
            return Succeed("角色保存成功。");
        }, "Save role failed");
    }

    public async Task<(bool Success, string Message)> DeleteSelectedRoleAsync()
    {
        return await RunOperationAsync(async () =>
        {
            if (SelectedRole == null)
            {
                return Fail("请先选择角色。");
            }

            if (IsSeedAdministratorRole(SelectedRole))
            {
                return Fail("删除被阻止：种子管理员角色不能删除。");
            }

            var accountResult = await _accountService.QueryAccountsAsync(page: 1, pageSize: int.MaxValue);
            var referencedAccounts = accountResult.Items
                .Where(account => account.SysRoleId == SelectedRole.SrId)
                .ToList();
            if (referencedAccounts.Count > 0)
            {
                var accountNames = string.Join(", ", referencedAccounts
                    .Take(3)
                    .Select(account => account.SysAccountName ?? account.SysNickname ?? account.SysId.ToString()));
                var suffix = referencedAccounts.Count > 3 ? " 等" : string.Empty;
                return Fail($"删除被阻止：该角色仍被 {referencedAccounts.Count} 个账户使用（{accountNames}{suffix}），请先迁移账户。");
            }

            var roleId = SelectedRole.SrId;
            var deleted = await _roleService.DeleteRoleAsync(roleId);
            if (!deleted)
            {
                return Fail("角色删除失败：角色不存在、仍被引用或服务拒绝删除。");
            }

            SelectedRole = null;
            await ReloadRolesCoreAsync();
            await LoadMenuPermissionsCoreAsync();
            return Succeed("角色删除成功。");
        }, "Delete role failed");
    }

    public async Task<(bool Success, string Message)> SavePermissionsAsync()
    {
        return await RunOperationAsync(async () =>
        {
            if (SelectedRole == null)
            {
                return Fail("请先选择角色。");
            }

            var selectedMenuIds = SelectedMenuIds.ToArray();
            var saved = await _roleService.AssignMenusToRoleAsync(SelectedRole.SrId, selectedMenuIds);
            if (!saved)
            {
                return Fail("权限保存失败：请确认角色和菜单仍然存在。");
            }

            await LoadMenuPermissionsCoreAsync();
            return Succeed("权限保存成功。已打开的会话菜单不会自动刷新，请重新登录后生效。");
        }, "Save role permissions failed");
    }

    private async Task LoadMenuPermissionsAsync()
    {
        await ExecuteAsync(LoadMenuPermissionsCoreAsync, ex =>
        {
            Debug.Error("Load role permissions failed", ex);
            MenuPermissions = new ObservableCollection<RoleMenuPermissionItem>();
        });
    }

    private async Task ReloadRolesCoreAsync(long? preferredRoleId = null)
    {
        var roles = (await _roleService.GetAllRolesAsync())
            .OrderBy(role => role.SrId)
            .ToList();

        _allRoles.Clear();
        _allRoles.AddRange(roles);
        ApplyRoleFilter();

        var roleIdToSelect = preferredRoleId ?? SelectedRole?.SrId;
        SelectedRole = roleIdToSelect.HasValue
            ? Roles.FirstOrDefault(role => role.SrId == roleIdToSelect.Value) ?? Roles.FirstOrDefault()
            : Roles.FirstOrDefault();

        StatusMessage = $"已加载 {Roles.Count} 个角色。";
    }

    private async Task ReloadMenusCoreAsync()
    {
        var menus = (await _menuService.GetAllMenusAsync()).ToList();
        _allMenus.Clear();
        _allMenus.AddRange(menus);
    }

    private async Task LoadMenuPermissionsCoreAsync()
    {
        if (_allMenus.Count == 0)
        {
            await ReloadMenusCoreAsync();
        }

        HashSet<long> selectedMenuIds = new();
        if (SelectedRole != null)
        {
            selectedMenuIds = (await _permissionService.GetRoleMenuIdsAsync(SelectedRole.SrId)).ToHashSet();
        }

        MenuPermissions = new ObservableCollection<RoleMenuPermissionItem>(
            FlattenMenus(_allMenus).Select(item => new RoleMenuPermissionItem(
                item.Menu,
                item.Level,
                selectedMenuIds.Contains(item.Menu.SmId))));

        StatusMessage = SelectedRole == null
            ? "请选择角色后分配菜单权限。"
            : $"已加载角色“{SelectedRole.SrName}”的菜单权限。";
    }

    private void ApplyRoleFilter()
    {
        IEnumerable<SysRoleModel> query = _allRoles;
        if (!string.IsNullOrWhiteSpace(SearchKeyword))
        {
            var keyword = SearchKeyword.Trim();
            query = query.Where(role =>
                Contains(role.SrName, keyword) ||
                Contains(role.SrEnName, keyword) ||
                Contains(role.SrRemark, keyword) ||
                Contains(GetStatusText(role.SrStatus), keyword));
        }

        Roles = new ObservableCollection<SysRoleModel>(query.OrderBy(role => role.SrId));
    }

    private void SelectFilteredRole()
    {
        if (SelectedRole != null && Roles.Any(role => role.SrId == SelectedRole.SrId))
        {
            return;
        }

        SelectedRole = Roles.FirstOrDefault();
    }

    private void LoadRoleForEdit()
    {
        EditRoleId = SelectedRole?.SrId ?? 0;
        EditName = SelectedRole?.SrName ?? string.Empty;
        EditEnName = SelectedRole?.SrEnName ?? string.Empty;
        EditRemark = SelectedRole?.SrRemark ?? string.Empty;
        EditStatus = SelectedRole?.SrStatus != true;
    }

    private (bool Success, string Message) Succeed(string message)
    {
        StatusMessage = message;
        OperationCompleted?.Invoke(this, message);
        return (true, message);
    }

    private (bool Success, string Message) Fail(string message)
    {
        StatusMessage = message;
        return (false, message);
    }

    private async Task<(bool Success, string Message)> RunOperationAsync(
        Func<Task<(bool Success, string Message)>> operation,
        string errorLog)
    {
        if (IsBusy)
        {
            return Fail("当前操作尚未完成，请稍后再试。");
        }

        try
        {
            IsBusy = true;
            return await operation();
        }
        catch (Exception ex)
        {
            Debug.Error(errorLog, ex);
            var message = FormatExceptionMessage(ex);
            StatusMessage = message;
            return (false, message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static void NormalizeRole(SysRoleModel role)
    {
        role.SrName = role.SrName.Trim();
        role.SrEnName = role.SrEnName.Trim();
        role.SrRemark = role.SrRemark.Trim();
        role.SrUpdateAt = DateTime.Now;
        role.SrCreateAt ??= DateTime.Now;
        role.SrReserved1 ??= string.Empty;
        role.SrReserved2 ??= string.Empty;
        role.SrReserved3 ??= string.Empty;
    }

    private static void ValidateRole(SysRoleModel role)
    {
        if (string.IsNullOrWhiteSpace(role.SrName))
        {
            throw new InvalidOperationException("角色名称不能为空。");
        }

        if (string.IsNullOrWhiteSpace(role.SrEnName))
        {
            throw new InvalidOperationException("英文名不能为空。");
        }

        if (role.SrName.Length > 64)
        {
            throw new InvalidOperationException("角色名称不能超过 64 个字符。");
        }

        if (role.SrEnName.Length > 64)
        {
            throw new InvalidOperationException("英文名不能超过 64 个字符。");
        }

        if (role.SrRemark.Length > 255)
        {
            throw new InvalidOperationException("备注不能超过 255 个字符。");
        }
    }

    private static bool IsSeedAdministratorRole(SysRoleModel role)
    {
        return role.SrId == 1 ||
               string.Equals(role.SrEnName, "Admin", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(role.SrName, "Administrator", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatExceptionMessage(Exception ex)
    {
        return ex is DataSourceUnavailableException
            ? "未连接后端，请检查数据源配置或启动 WebApi 服务。"
            : ex.Message;
    }

    private static string GetStatusText(bool? status)
    {
        return status == true ? "停用" : "启用";
    }

    private static bool Contains(string? value, string keyword)
    {
        return value?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true;
    }

    private static IReadOnlyList<(SysMenuModel Menu, int Level)> FlattenMenus(IEnumerable<SysMenuModel> menus)
    {
        var orderedMenus = menus
            .OrderBy(menu => menu.SmSort ?? int.MaxValue)
            .ThenBy(menu => menu.SmId)
            .ToList();
        var byId = orderedMenus.ToDictionary(menu => menu.SmId);
        var children = orderedMenus
            .GroupBy(menu => menu.SmParentId)
            .ToDictionary(group => group.Key, group => group.ToList());
        var result = new List<(SysMenuModel Menu, int Level)>();
        var visited = new HashSet<long>();

        void AddWithChildren(SysMenuModel menu, int level)
        {
            if (!visited.Add(menu.SmId))
            {
                return;
            }

            result.Add((menu, level));
            if (!children.TryGetValue(menu.SmId, out var childMenus))
            {
                return;
            }

            foreach (var child in childMenus)
            {
                AddWithChildren(child, level + 1);
            }
        }

        foreach (var root in orderedMenus.Where(menu => menu.SmParentId == 0 || !byId.ContainsKey(menu.SmParentId)))
        {
            AddWithChildren(root, 0);
        }

        foreach (var menu in orderedMenus)
        {
            AddWithChildren(menu, 0);
        }

        return result;
    }
}

public sealed class RoleMenuPermissionItem : ObservableObject
{
    private bool _isChecked;

    public RoleMenuPermissionItem(SysMenuModel menu, int level, bool isChecked)
    {
        ArgumentNullException.ThrowIfNull(menu);

        MenuId = menu.SmId;
        ParentId = menu.SmParentId;
        Name = menu.SmName ?? string.Empty;
        EnName = menu.SmEnName ?? string.Empty;
        Url = menu.SmUrl ?? string.Empty;
        Sort = menu.SmSort ?? int.MaxValue;
        Level = Math.Max(level, 0);
        IsGroup = string.Equals(Url, "#", StringComparison.Ordinal);
        StatusText = menu.SysStatus == true ? "停用" : "启用";
        _isChecked = isChecked;
    }

    public long MenuId { get; }

    public long ParentId { get; }

    public string Name { get; }

    public string EnName { get; }

    public string Url { get; }

    public int Sort { get; }

    public int Level { get; }

    public bool IsGroup { get; }

    public string StatusText { get; }

    public string DisplayName => $"{new string(' ', Level * 2)}{Name}";

    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }
}
