using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.Repositories;
using WinformTemplate.Business.Sys.Service;

namespace WinformTemplate.Tests.Business.Sys;

public class EfSysAccountRepositoryTests
{
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
    public async Task GetByUsernameAsync_UsesSqlWhere()
    {
        var logs = new List<string>();
        var dbPath = Path.Combine(_tempDirectory!, "sys.db");
        var options = new DbContextOptionsBuilder<SysDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .EnableSensitiveDataLogging()
            .LogTo(logs.Add, LogLevel.Information)
            .Options;

        await using var context = new SysDbContext(options);
        await context.Database.EnsureCreatedAsync();
        context.SysAccounts.Add(new SysAccountModel
        {
            SysAccountName = "admin",
            SysPassword = "hash",
            SysNickname = "管理员",
            SysStatus = false,
            SysCreateAt = DateTime.Now
        });
        await context.SaveChangesAsync();

        var repository = new EfSysAccountRepository(context);
        var account = await repository.GetByUsernameAsync("admin");

        Assert.IsNotNull(account);
        var sqlLog = string.Join(Environment.NewLine, logs);
        Assert.That(sqlLog, Does.Contain("WHERE"));
        Assert.That(sqlLog, Does.Contain("sys_account"));
    }

    [Test]
    public async Task DefaultSqliteSeed_AllowsAdminLoginAndLoadsMenus()
    {
        var dbPath = Path.Combine(_tempDirectory!, "app.db");
        var options = new DbContextOptionsBuilder<SysDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        await using var context = new SysDbContext(options);
        var initializer = new SysDatabaseInitializer(context);
        await initializer.InitializeAsync();

        var accountRepository = new EfSysAccountRepository(context);
        var roleRepository = new EfSysRoleRepository(context);
        var menuRepository = new EfSysMenuRepository(context);
        var accountService = new SysAccountService(accountRepository, roleRepository, menuRepository);
        var permissionService = new PermissionService(accountRepository, roleRepository, menuRepository);

        var account = await accountService.LoginAsync("admin", "123456");
        Assert.IsNotNull(account);

        var menus = (await permissionService.GetAccessibleMenuTreeAsync(account!.SysId)).ToList();
        Assert.That(menus.Count, Is.GreaterThanOrEqualTo(3));
    }
}
