using System.Text.Json;
using NUnit.Framework;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.Repositories;
using WinformTemplate.Business.Sys.Service;

namespace WinformTemplate.Tests.Business.Sys;

public sealed class LocalSysRepositoryTests
{
    private const string AdminPasswordHash = "PBKDF2$100000$T7Kd0QC8zeX7VUmRe1EgKw==$gyG1FfFJ8Gh5WWzrFHC1zUoJn83rjt/Zp5oSrCi0C+4=";
    private string? _tempDirectory;

    [SetUp]
    public void Setup()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "WinformTemplate.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
        WriteSeedFiles(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (!string.IsNullOrWhiteSpace(_tempDirectory) && Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Test]
    public async Task LocalSysRepositories_AllowAdminLoginAndMenuLoading()
    {
        var accountRepository = new LocalSysAccountRepository(_tempDirectory!);
        var menuRepository = new LocalSysMenuRepository(_tempDirectory!);
        var roleAuthRepository = new LocalSysRoleAuthRepository(_tempDirectory!);
        var roleRepository = new LocalSysRoleRepository(_tempDirectory!);
        var accountService = new SysAccountService(accountRepository, roleRepository, menuRepository);
        var permissionService = new PermissionService(accountRepository, roleRepository, menuRepository);

        var account = await accountService.LoginAsync("admin", "123456");
        Assert.That(account, Is.Not.Null);

        var menus = (await permissionService.GetAccessibleMenuTreeAsync(account!.SysId)).ToList();
        Assert.That(menus.Count, Is.EqualTo(3));
        Assert.That(await roleAuthRepository.HasPermissionAsync(1, 2), Is.True);
    }

    private static void WriteSeedFiles(string seedRoot)
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
            }
        });
        WriteJson(Path.Combine(seedRoot, "sysRoles.json"), new[]
        {
            new SysRoleModel
            {
                SrId = 1,
                SrName = "Administrator",
                SrEnName = "Admin",
                SrRemark = "Local admin",
                SrStatus = false,
                SrCreateAt = DateTime.Today
            }
        });
        WriteJson(Path.Combine(seedRoot, "sysMenus.json"), new[]
        {
            new SysMenuModel { SmId = 1, SmParentId = 0, SmName = "System", SmEnName = "System", SmType = 0, SmSort = 1, SysStatus = false },
            new SysMenuModel { SmId = 2, SmParentId = 1, SmName = "Accounts", SmEnName = "Accounts", SmType = 0, SmSort = 1, SysStatus = false },
            new SysMenuModel { SmId = 3, SmParentId = 1, SmName = "Roles", SmEnName = "Roles", SmType = 0, SmSort = 2, SysStatus = false }
        });
        WriteJson(Path.Combine(seedRoot, "sysRoleAuths.json"), new[]
        {
            new SysRoleAuthModel { SraRoleId = 1, SraMenuId = 1 },
            new SysRoleAuthModel { SraRoleId = 1, SraMenuId = 2 },
            new SysRoleAuthModel { SraRoleId = 1, SraMenuId = 3 }
        });
        WriteJson(Path.Combine(seedRoot, "sysParams.json"), Array.Empty<SysParamModel>());
    }

    private static void WriteJson<T>(string path, T value)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }));
    }
}
