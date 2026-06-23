using NUnit.Framework;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.Service;
using WinformTemplate.Business.Sys.ViewModel;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Tests.Business.Sys;

public sealed class AccountManagementViewModelTests
{
    [Test]
    public async Task LoadAccountsAsync_UsesPagedQuery()
    {
        var accountService = new FakeAccountService();
        var viewModel = CreateViewModel(accountService);

        viewModel.PageSize = 2;
        await viewModel.LoadAccountsAsync();

        Assert.That(accountService.LastQuery, Is.EqualTo(new AccountQuery(null, 1, 2)));
        Assert.That(viewModel.TotalCount, Is.EqualTo(5));
        Assert.That(viewModel.TotalPages, Is.EqualTo(3));
        Assert.That(viewModel.Accounts.Select(account => account.SysId), Is.EqualTo(new[] { 1L, 2L }));
    }

    [Test]
    public async Task InitializeAsync_LoadsRolesAndFirstAccountPage()
    {
        var accountService = new FakeAccountService();
        var viewModel = CreateViewModel(accountService);

        await viewModel.InitializeAsync();

        Assert.That(viewModel.Roles.Select(role => role.SrName), Is.EqualTo(new[] { "Admin", "User" }));
        Assert.That(accountService.LastQuery, Is.EqualTo(new AccountQuery(null, 1, 20)));
        Assert.That(viewModel.Accounts.Count, Is.EqualTo(5));
    }

    [Test]
    public async Task SearchAsync_ResetsToFirstPageAndPassesKeyword()
    {
        var accountService = new FakeAccountService();
        var viewModel = CreateViewModel(accountService);

        viewModel.PageSize = 2;
        await viewModel.LoadAccountsAsync();
        await viewModel.GoToPageAsync(2);

        viewModel.SearchKeyword = "admin";
        await viewModel.SearchAsync();

        Assert.That(accountService.LastQuery, Is.EqualTo(new AccountQuery("admin", 1, 2)));
        Assert.That(viewModel.CurrentPage, Is.EqualTo(1));
        Assert.That(viewModel.TotalCount, Is.EqualTo(1));
        Assert.That(viewModel.Accounts.Single().SysAccountName, Is.EqualTo("admin"));
    }

    [Test]
    public async Task GoToPageAsync_LoadsRequestedPage()
    {
        var accountService = new FakeAccountService();
        var viewModel = CreateViewModel(accountService);

        viewModel.PageSize = 2;
        await viewModel.LoadAccountsAsync();
        await viewModel.GoToPageAsync(2);

        Assert.That(accountService.LastQuery, Is.EqualTo(new AccountQuery(null, 2, 2)));
        Assert.That(viewModel.CurrentPage, Is.EqualTo(2));
        Assert.That(viewModel.Accounts.Select(account => account.SysId), Is.EqualTo(new[] { 3L, 4L }));
    }

    private static AccountManagementViewModel CreateViewModel(FakeAccountService accountService)
    {
        return new AccountManagementViewModel(accountService, new FakeRoleService());
    }

    private sealed class FakeAccountService : ISysAccountService
    {
        private readonly List<SysAccountModel> _accounts = new()
        {
            new SysAccountModel { SysId = 1, SysAccountName = "admin", SysNickname = "Administrator", SysRoleId = 1, SysStatus = true },
            new SysAccountModel { SysId = 2, SysAccountName = "alice", SysNickname = "Alice", SysRoleId = 2, SysStatus = true },
            new SysAccountModel { SysId = 3, SysAccountName = "bob", SysNickname = "Bob", SysRoleId = 2, SysStatus = true },
            new SysAccountModel { SysId = 4, SysAccountName = "carol", SysNickname = "Carol", SysRoleId = 2, SysStatus = false },
            new SysAccountModel { SysId = 5, SysAccountName = "dave", SysNickname = "Dave", SysRoleId = 2, SysStatus = true }
        };

        public AccountQuery? LastQuery { get; private set; }

        public Task<SysAccountModel?> LoginAsync(string username, string password)
        {
            return Task.FromResult(_accounts.FirstOrDefault(account => account.SysAccountName == username));
        }

        public Task<IEnumerable<SysAccountModel>> GetAllAccountsAsync()
        {
            return Task.FromResult<IEnumerable<SysAccountModel>>(_accounts.ToList());
        }

        public Task<IEnumerable<SysAccountModel>> SearchAccountsAsync(string keyword)
        {
            return Task.FromResult<IEnumerable<SysAccountModel>>(Filter(keyword).ToList());
        }

        public Task<PagedResult<SysAccountModel>> QueryAccountsAsync(string? keyword = null, int page = 1, int pageSize = 20)
        {
            LastQuery = new AccountQuery(keyword, page, pageSize);
            var query = Filter(keyword);
            var total = query.Count();
            var items = query
                .Skip((Math.Max(page, 1) - 1) * Math.Max(pageSize, 1))
                .Take(Math.Max(pageSize, 1))
                .ToList();

            return Task.FromResult(new PagedResult<SysAccountModel>
            {
                Items = items,
                Total = total
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
            account.SysId = _accounts.Max(item => item.SysId) + 1;
            _accounts.Add(account);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateAccountAsync(SysAccountModel account)
        {
            var index = _accounts.FindIndex(item => item.SysId == account.SysId);
            if (index < 0)
            {
                return Task.FromResult(false);
            }

            _accounts[index] = account;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAccountAsync(long id)
        {
            return Task.FromResult(_accounts.RemoveAll(account => account.SysId == id) > 0);
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

        private IEnumerable<SysAccountModel> Filter(string? keyword)
        {
            IEnumerable<SysAccountModel> query = _accounts;
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(account =>
                    account.SysAccountName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true ||
                    account.SysNickname?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true);
            }

            return query.OrderBy(account => account.SysId);
        }
    }

    private sealed class FakeRoleService : ISysRoleService
    {
        private readonly List<SysRoleModel> _roles = new()
        {
            new SysRoleModel { SrId = 1, SrName = "Admin" },
            new SysRoleModel { SrId = 2, SrName = "User" }
        };

        public Task<IEnumerable<SysRoleModel>> GetAllRolesAsync()
        {
            return Task.FromResult<IEnumerable<SysRoleModel>>(_roles);
        }

        public Task<SysRoleModel?> GetRoleByIdAsync(long id)
        {
            return Task.FromResult(_roles.FirstOrDefault(role => role.SrId == id));
        }

        public Task<bool> CreateRoleAsync(SysRoleModel role)
        {
            _roles.Add(role);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateRoleAsync(SysRoleModel role)
        {
            return Task.FromResult(true);
        }

        public Task<bool> DeleteRoleAsync(long id)
        {
            return Task.FromResult(true);
        }

        public Task<bool> AssignMenuToRoleAsync(long roleId, long menuId)
        {
            return Task.FromResult(true);
        }

        public Task<bool> AssignMenusToRoleAsync(long roleId, IEnumerable<long> menuIds)
        {
            return Task.FromResult(true);
        }

        public Task<bool> RemoveMenuFromRoleAsync(long roleId, long menuId)
        {
            return Task.FromResult(true);
        }

        public Task<IEnumerable<SysMenuModel>> GetRoleMenusAsync(long roleId)
        {
            return Task.FromResult<IEnumerable<SysMenuModel>>(Array.Empty<SysMenuModel>());
        }

        public Task<bool> HasMenuPermissionAsync(long roleId, long menuId)
        {
            return Task.FromResult(true);
        }
    }

    public sealed record AccountQuery(string? Keyword, int Page, int PageSize);
}
