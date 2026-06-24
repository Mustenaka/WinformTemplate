using NUnit.Framework;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.Service;
using WinformTemplate.Business.Sys.ViewModel;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Tests.Business.Sys;

public sealed class RoleManagementViewModelTests
{
    [Test]
    public async Task InitializeAsync_LoadsRolesMenusAndAuthorizedIds()
    {
        var services = new FakeSysServices();
        var viewModel = CreateViewModel(services);

        await viewModel.InitializeAsync();

        Assert.That(viewModel.Roles.Select(role => role.SrName), Is.EqualTo(new[] { "Administrator", "Operator" }));
        Assert.That(viewModel.SelectedRole?.SrId, Is.EqualTo(1));
        Assert.That(viewModel.MenuPermissions.Select(menu => menu.MenuId), Is.EqualTo(new[] { 1L, 2L, 3L }));
        Assert.That(viewModel.SelectedMenuIds, Is.EquivalentTo(new[] { 1L, 2L }));
    }

    [Test]
    public async Task SaveRoleAsync_CreatesUpdatesAndDeletesRole()
    {
        var services = new FakeSysServices();
        var viewModel = CreateViewModel(services);
        await viewModel.InitializeAsync();

        var createResult = await viewModel.SaveRoleAsync(new SysRoleModel
        {
            SrName = "Auditor",
            SrEnName = "Auditor",
            SrRemark = "Read only",
            SrStatus = false
        });

        Assert.That(createResult.Success, Is.True);
        var created = services.RoleService.Roles.Single(role => role.SrEnName == "Auditor");
        Assert.That(created.SrId, Is.GreaterThan(2));

        created.SrRemark = "Audit only";
        var updateResult = await viewModel.SaveRoleAsync(created);

        Assert.That(updateResult.Success, Is.True);
        Assert.That(services.RoleService.Roles.Single(role => role.SrId == created.SrId).SrRemark, Is.EqualTo("Audit only"));

        await viewModel.SelectRoleAsync(created);
        var deleteResult = await viewModel.DeleteSelectedRoleAsync();

        Assert.That(deleteResult.Success, Is.True);
        Assert.That(services.RoleService.Roles.Any(role => role.SrId == created.SrId), Is.False);
    }

    [Test]
    public async Task SavePermissionsAsync_CallsAssignMenusWithCheckedMenuIds()
    {
        var services = new FakeSysServices();
        var viewModel = CreateViewModel(services);
        await viewModel.InitializeAsync();

        await viewModel.SelectRoleAsync(viewModel.Roles.Single(role => role.SrId == 2));
        viewModel.SetMenuChecked(1, true);
        viewModel.SetMenuChecked(2, true);
        viewModel.SetMenuChecked(3, false);

        var result = await viewModel.SavePermissionsAsync();

        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Does.Contain("重新登录"));
        Assert.That(services.RoleService.LastAssignedRoleId, Is.EqualTo(2));
        Assert.That(services.RoleService.LastAssignedMenuIds, Is.EquivalentTo(new[] { 1L, 2L }));
    }

    [Test]
    public async Task DeleteSelectedRoleAsync_BlocksRoleReferencedByAccount()
    {
        var services = new FakeSysServices();
        var viewModel = CreateViewModel(services);
        await viewModel.InitializeAsync();

        await viewModel.SelectRoleAsync(viewModel.Roles.Single(role => role.SrId == 2));
        var result = await viewModel.DeleteSelectedRoleAsync();

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("仍被 1 个账户使用"));
        Assert.That(services.RoleService.DeleteCalledRoleIds, Does.Not.Contain(2L));
    }

    private static RoleManagementViewModel CreateViewModel(FakeSysServices services)
    {
        return new RoleManagementViewModel(
            services.RoleService,
            services.MenuService,
            services.PermissionService,
            services.AccountService);
    }

    private sealed class FakeSysServices
    {
        public FakeSysServices()
        {
            RoleService = new FakeRoleService();
            MenuService = new FakeMenuService();
            PermissionService = new FakePermissionService(RoleService);
            AccountService = new FakeAccountService();
        }

        public FakeRoleService RoleService { get; }

        public FakeMenuService MenuService { get; }

        public FakePermissionService PermissionService { get; }

        public FakeAccountService AccountService { get; }
    }

    private sealed class FakeRoleService : ISysRoleService
    {
        private long _nextRoleId = 3;

        public List<SysRoleModel> Roles { get; } = new()
        {
            new SysRoleModel { SrId = 1, SrName = "Administrator", SrEnName = "Admin", SrStatus = false },
            new SysRoleModel { SrId = 2, SrName = "Operator", SrEnName = "Operator", SrStatus = false }
        };

        public Dictionary<long, HashSet<long>> RoleMenuIds { get; } = new()
        {
            [1] = new HashSet<long> { 1, 2 },
            [2] = new HashSet<long> { 2 }
        };

        public long? LastAssignedRoleId { get; private set; }

        public IReadOnlyList<long> LastAssignedMenuIds { get; private set; } = Array.Empty<long>();

        public List<long> DeleteCalledRoleIds { get; } = new();

        public Task<IEnumerable<SysRoleModel>> GetAllRolesAsync()
        {
            return Task.FromResult<IEnumerable<SysRoleModel>>(Roles.Select(CloneRole).ToList());
        }

        public Task<SysRoleModel?> GetRoleByIdAsync(long id)
        {
            return Task.FromResult(Roles.FirstOrDefault(role => role.SrId == id));
        }

        public Task<bool> CreateRoleAsync(SysRoleModel role)
        {
            role.SrId = _nextRoleId++;
            Roles.Add(CloneRole(role));
            RoleMenuIds[role.SrId] = new HashSet<long>();
            return Task.FromResult(true);
        }

        public Task<bool> UpdateRoleAsync(SysRoleModel role)
        {
            var index = Roles.FindIndex(item => item.SrId == role.SrId);
            if (index < 0)
            {
                return Task.FromResult(false);
            }

            Roles[index] = CloneRole(role);
            return Task.FromResult(true);
        }

        public Task<bool> DeleteRoleAsync(long id)
        {
            DeleteCalledRoleIds.Add(id);
            RoleMenuIds.Remove(id);
            return Task.FromResult(Roles.RemoveAll(role => role.SrId == id) > 0);
        }

        public Task<bool> AssignMenuToRoleAsync(long roleId, long menuId)
        {
            if (!RoleMenuIds.TryGetValue(roleId, out var menuIds))
            {
                menuIds = new HashSet<long>();
                RoleMenuIds[roleId] = menuIds;
            }

            menuIds.Add(menuId);
            return Task.FromResult(true);
        }

        public Task<bool> AssignMenusToRoleAsync(long roleId, IEnumerable<long> menuIds)
        {
            LastAssignedRoleId = roleId;
            LastAssignedMenuIds = menuIds.ToArray();
            RoleMenuIds[roleId] = LastAssignedMenuIds.ToHashSet();
            return Task.FromResult(true);
        }

        public Task<bool> RemoveMenuFromRoleAsync(long roleId, long menuId)
        {
            if (RoleMenuIds.TryGetValue(roleId, out var menuIds))
            {
                menuIds.Remove(menuId);
            }

            return Task.FromResult(true);
        }

        public Task<IEnumerable<SysMenuModel>> GetRoleMenusAsync(long roleId)
        {
            return Task.FromResult<IEnumerable<SysMenuModel>>(Array.Empty<SysMenuModel>());
        }

        public Task<bool> HasMenuPermissionAsync(long roleId, long menuId)
        {
            return Task.FromResult(RoleMenuIds.TryGetValue(roleId, out var menuIds) && menuIds.Contains(menuId));
        }

        private static SysRoleModel CloneRole(SysRoleModel role)
        {
            return new SysRoleModel
            {
                SrId = role.SrId,
                SrName = role.SrName,
                SrEnName = role.SrEnName,
                SrRemark = role.SrRemark,
                SrStatus = role.SrStatus,
                SrCreateAt = role.SrCreateAt,
                SrUpdateAt = role.SrUpdateAt,
                SrReserved1 = role.SrReserved1,
                SrReserved2 = role.SrReserved2,
                SrReserved3 = role.SrReserved3
            };
        }
    }

    private sealed class FakeMenuService : ISysMenuService
    {
        private readonly List<SysMenuModel> _menus = new()
        {
            new SysMenuModel { SmId = 1, SmParentId = 0, SmName = "System", SmEnName = "System", SmUrl = "#", SmSort = 1, SysStatus = false },
            new SysMenuModel { SmId = 2, SmParentId = 1, SmName = "Accounts", SmEnName = "Accounts", SmUrl = "/sys/user", SmSort = 1, SysStatus = false },
            new SysMenuModel { SmId = 3, SmParentId = 1, SmName = "Roles", SmEnName = "Roles", SmUrl = "/sys/role", SmSort = 2, SysStatus = false }
        };

        public Task<IEnumerable<SysMenuModel>> GetAllMenusAsync()
        {
            return Task.FromResult<IEnumerable<SysMenuModel>>(_menus);
        }

        public Task<IEnumerable<SysMenuModel>> GetMenuTreeAsync()
        {
            return GetAllMenusAsync();
        }

        public Task<IEnumerable<SysMenuModel>> GetMenusByRoleIdAsync(long roleId)
        {
            return Task.FromResult<IEnumerable<SysMenuModel>>(Array.Empty<SysMenuModel>());
        }

        public Task<IEnumerable<SysMenuModel>> GetMenusByAccountIdAsync(long accountId)
        {
            return Task.FromResult<IEnumerable<SysMenuModel>>(Array.Empty<SysMenuModel>());
        }

        public Task<SysMenuModel?> GetMenuByIdAsync(long id)
        {
            return Task.FromResult(_menus.FirstOrDefault(menu => menu.SmId == id));
        }

        public Task<bool> CreateMenuAsync(SysMenuModel menu)
        {
            return Task.FromResult(true);
        }

        public Task<bool> UpdateMenuAsync(SysMenuModel menu)
        {
            return Task.FromResult(true);
        }

        public Task<bool> DeleteMenuAsync(long id)
        {
            return Task.FromResult(true);
        }

        public Task<bool> FreezeMenuAsync(long id)
        {
            return Task.FromResult(true);
        }

        public Task<bool> UnfreezeMenuAsync(long id)
        {
            return Task.FromResult(true);
        }

        public Task<IEnumerable<SysMenuModel>> GetChildMenusAsync(long parentId)
        {
            return Task.FromResult<IEnumerable<SysMenuModel>>(_menus.Where(menu => menu.SmParentId == parentId));
        }
    }

    private sealed class FakePermissionService : IPermissionService
    {
        private readonly FakeRoleService _roleService;

        public FakePermissionService(FakeRoleService roleService)
        {
            _roleService = roleService;
        }

        public Task<bool> HasPermissionAsync(long accountId, long menuId)
        {
            return Task.FromResult(true);
        }

        public Task<bool> HasPermissionAsync(long accountId, string menuKey)
        {
            return Task.FromResult(true);
        }

        public Task<IEnumerable<SysMenuModel>> GetAccessibleMenusAsync(long accountId)
        {
            return Task.FromResult<IEnumerable<SysMenuModel>>(Array.Empty<SysMenuModel>());
        }

        public Task<IEnumerable<SysMenuModel>> GetAccessibleMenuTreeAsync(long accountId)
        {
            return Task.FromResult<IEnumerable<SysMenuModel>>(Array.Empty<SysMenuModel>());
        }

        public IEnumerable<SysMenuModel> FilterMenuTree(IEnumerable<SysMenuModel> allMenus, IEnumerable<long> accessibleMenuIds)
        {
            return allMenus;
        }

        public Task<bool> IsAccountValidAsync(long accountId)
        {
            return Task.FromResult(true);
        }

        public Task<IEnumerable<long>> GetRoleMenuIdsAsync(long roleId)
        {
            return Task.FromResult<IEnumerable<long>>(
                _roleService.RoleMenuIds.TryGetValue(roleId, out var menuIds) ? menuIds.ToArray() : Array.Empty<long>());
        }
    }

    private sealed class FakeAccountService : ISysAccountService
    {
        private readonly List<SysAccountModel> _accounts = new()
        {
            new SysAccountModel { SysId = 1, SysAccountName = "admin", SysRoleId = 1, SysStatus = false },
            new SysAccountModel { SysId = 2, SysAccountName = "operator", SysRoleId = 2, SysStatus = false }
        };

        public Task<SysAccountModel?> LoginAsync(string username, string password)
        {
            return Task.FromResult(_accounts.FirstOrDefault(account => account.SysAccountName == username));
        }

        public Task<IEnumerable<SysAccountModel>> GetAllAccountsAsync()
        {
            return Task.FromResult<IEnumerable<SysAccountModel>>(_accounts);
        }

        public Task<IEnumerable<SysAccountModel>> SearchAccountsAsync(string keyword)
        {
            return Task.FromResult<IEnumerable<SysAccountModel>>(_accounts);
        }

        public Task<PagedResult<SysAccountModel>> QueryAccountsAsync(string? keyword = null, int page = 1, int pageSize = 20)
        {
            return Task.FromResult(new PagedResult<SysAccountModel>
            {
                Items = _accounts,
                Total = _accounts.Count
            });
        }

        public Task<SysAccountModel?> GetAccountByIdAsync(long id)
        {
            return Task.FromResult(_accounts.FirstOrDefault(account => account.SysId == id));
        }

        public Task<SysAccountModel?> GetAccountByUsernameAsync(string username)
        {
            return Task.FromResult(_accounts.FirstOrDefault(account => account.SysAccountName == username));
        }

        public Task<bool> CreateAccountAsync(SysAccountModel account)
        {
            return Task.FromResult(true);
        }

        public Task<bool> UpdateAccountAsync(SysAccountModel account)
        {
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAccountAsync(long id)
        {
            return Task.FromResult(true);
        }

        public Task<bool> ChangePasswordAsync(long accountId, string oldPassword, string newPassword)
        {
            return Task.FromResult(true);
        }

        public Task<bool> FreezeAccountAsync(long accountId)
        {
            return Task.FromResult(true);
        }

        public Task<bool> UnfreezeAccountAsync(long accountId)
        {
            return Task.FromResult(true);
        }

        public Task<bool> HasPermissionAsync(long accountId, long menuId)
        {
            return Task.FromResult(true);
        }

        public Task<IEnumerable<SysMenuModel>> GetAccountMenusAsync(long accountId)
        {
            return Task.FromResult<IEnumerable<SysMenuModel>>(Array.Empty<SysMenuModel>());
        }
    }
}
