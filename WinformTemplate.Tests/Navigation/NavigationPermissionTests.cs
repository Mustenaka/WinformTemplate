using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.Repositories;
using WinformTemplate.Business.Sys.Service;
using WinformTemplate.Navigation;

namespace WinformTemplate.Tests.Navigation;

[Apartment(ApartmentState.STA)]
public sealed class NavigationPermissionTests
{
    private const string AdminPasswordHash = "PBKDF2$100000$T7Kd0QC8zeX7VUmRe1EgKw==$gyG1FfFJ8Gh5WWzrFHC1zUoJn83rjt/Zp5oSrCi0C+4=";
    private string? _tempDirectory;

    [SetUp]
    public void Setup()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "WinformTemplate.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (!string.IsNullOrWhiteSpace(_tempDirectory) && Directory.Exists(_tempDirectory))
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Test]
    public async Task EfNavigation_AdminSeesAllOperatorIsRestricted()
    {
        var dbPath = Path.Combine(_tempDirectory!, "sys.db");
        var options = new DbContextOptionsBuilder<SysDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        await using var context = new SysDbContext(options);
        await new SysDatabaseInitializer(context).InitializeAsync();

        var accountRepository = new EfSysAccountRepository(context);
        var menuRepository = new EfSysMenuRepository(context);
        var roleRepository = new EfSysRoleRepository(context);
        var permissionService = new PermissionService(accountRepository, roleRepository, menuRepository);

        var admin = await accountRepository.GetByUsernameAsync("admin");
        var operatorAccount = await accountRepository.GetByUsernameAsync("operator");

        Assert.That(admin, Is.Not.Null);
        Assert.That(operatorAccount, Is.Not.Null);

        await AssertNavigationContractAsync(permissionService, admin!, operatorAccount!);
    }

    [Test]
    public async Task LocalNavigation_AdminSeesAllOperatorIsRestricted()
    {
        WriteLocalSeeds(_tempDirectory!);

        var accountRepository = new LocalSysAccountRepository(_tempDirectory!);
        var menuRepository = new LocalSysMenuRepository(_tempDirectory!);
        var roleRepository = new LocalSysRoleRepository(_tempDirectory!);
        var permissionService = new PermissionService(accountRepository, roleRepository, menuRepository);

        var admin = await accountRepository.GetByUsernameAsync("admin");
        var operatorAccount = await accountRepository.GetByUsernameAsync("operator");

        Assert.That(admin, Is.Not.Null);
        Assert.That(operatorAccount, Is.Not.Null);

        await AssertNavigationContractAsync(permissionService, admin!, operatorAccount!);
    }

    private static async Task AssertNavigationContractAsync(
        IPermissionService permissionService,
        SysAccountModel admin,
        SysAccountModel operatorAccount)
    {
        var adminMenus = (await permissionService.GetAccessibleMenuTreeAsync(admin.SysId)).ToList();
        var operatorMenus = (await permissionService.GetAccessibleMenuTreeAsync(operatorAccount.SysId)).ToList();

        Assert.That(RouteKeys(adminMenus), Is.EquivalentTo(new[] { "/sys/user", "/sys/role" }));
        Assert.That(RouteKeys(operatorMenus), Is.EquivalentTo(new[] { "/sys/user" }));

        var accessor = new CurrentAccountAccessor { CurrentAccount = admin };
        var navigationService = CreateNavigationService(accessor, permissionService);

        var adminUserResult = await navigationService.NavigateAsync("/sys/user");
        var adminRoleResult = await navigationService.NavigateAsync("/sys/role");
        Assert.That(adminUserResult.Status, Is.EqualTo(NavigationStatus.Success));
        Assert.That(adminRoleResult.Status, Is.EqualTo(NavigationStatus.Success));

        accessor.CurrentAccount = operatorAccount;

        var operatorUserResult = await navigationService.NavigateAsync("/sys/user");
        var operatorRoleResult = await navigationService.NavigateAsync("/sys/role");
        Assert.That(operatorUserResult.Status, Is.EqualTo(NavigationStatus.Success));
        Assert.That(operatorRoleResult.Status, Is.EqualTo(NavigationStatus.Unauthorized));
        Assert.That(operatorRoleResult.Message, Is.EqualTo("无权限"));
    }

    private static INavigationService CreateNavigationService(
        ICurrentAccountAccessor accessor,
        IPermissionService permissionService)
    {
        var registry = new PageRegistry();
        registry.Register("/sys/user", _ => new UserControl { Name = "UserPage" });
        registry.Register("/sys/role", _ => new UserControl { Name = "RolePage" });

        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        return new NavigationService(accessor, permissionService, registry, serviceProvider);
    }

    private static string[] RouteKeys(IEnumerable<SysMenuModel> menus)
    {
        return menus
            .Where(menu => !string.IsNullOrWhiteSpace(menu.SmUrl) && menu.SmUrl != "#")
            .Select(menu => menu.SmUrl)
            .OrderBy(key => key)
            .ToArray();
    }

    private static void WriteLocalSeeds(string seedRoot)
    {
        WriteJson(Path.Combine(seedRoot, "sysAccounts.json"), new[]
        {
            new SysAccountModel
            {
                SysId = 1,
                SysAccountName = "admin",
                SysPassword = AdminPasswordHash,
                SysNickname = "Admin",
                SysLevel = 0,
                SysRoleId = 1,
                SysStatus = false,
                SysCreateAt = DateTime.Today
            },
            new SysAccountModel
            {
                SysId = 2,
                SysAccountName = "operator",
                SysPassword = AdminPasswordHash,
                SysNickname = "Operator",
                SysLevel = 1,
                SysRoleId = 2,
                SysStatus = false,
                SysCreateAt = DateTime.Today
            }
        });

        WriteJson(Path.Combine(seedRoot, "sysRoles.json"), new[]
        {
            new SysRoleModel { SrId = 1, SrName = "Administrator", SrEnName = "Admin", SrStatus = false },
            new SysRoleModel { SrId = 2, SrName = "Operator", SrEnName = "Operator", SrStatus = false }
        });

        WriteJson(Path.Combine(seedRoot, "sysMenus.json"), new[]
        {
            new SysMenuModel { SmId = 1, SmParentId = 0, SmName = "System", SmEnName = "System", SmType = 0, SmUrl = "#", SmSort = 1, SysStatus = false },
            new SysMenuModel { SmId = 2, SmParentId = 1, SmName = "Accounts", SmEnName = "Accounts", SmType = 0, SmUrl = "/sys/user", SmSort = 1, SysStatus = false },
            new SysMenuModel { SmId = 3, SmParentId = 1, SmName = "Roles", SmEnName = "Roles", SmType = 0, SmUrl = "/sys/role", SmSort = 2, SysStatus = false }
        });

        WriteJson(Path.Combine(seedRoot, "sysRoleAuths.json"), new[]
        {
            new SysRoleAuthModel { SraRoleId = 1, SraMenuId = 1 },
            new SysRoleAuthModel { SraRoleId = 1, SraMenuId = 2 },
            new SysRoleAuthModel { SraRoleId = 1, SraMenuId = 3 },
            new SysRoleAuthModel { SraRoleId = 2, SraMenuId = 2 }
        });

        WriteJson(Path.Combine(seedRoot, "sysParams.json"), Array.Empty<SysParamModel>());
    }

    private static void WriteJson<T>(string path, T value)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }));
    }
}
