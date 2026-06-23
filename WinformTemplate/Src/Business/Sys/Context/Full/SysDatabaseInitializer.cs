using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Tools.Encryption;

namespace WinformTemplate.Business.Sys.Context;

public sealed class SysDatabaseInitializer : IDatabaseInitializer
{
    private readonly SysDbContext _context;

    public SysDatabaseInitializer(SysDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public string ModuleKey => "Sys";

    public DataSourceKind DataSourceKind => DataSourceKind.Ef;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.EnsureCreatedAsync(cancellationToken);

        if (await _context.SysRoles.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.Now;
        var adminRole = new SysRoleModel
        {
            SrName = "Administrator",
            SrEnName = "Admin",
            SrRemark = "System administrator role",
            SrReserved1 = string.Empty,
            SrReserved2 = string.Empty,
            SrReserved3 = string.Empty,
            SrStatus = false,
            SrCreateAt = now,
            SrUpdateAt = now
        };

        var operatorRole = new SysRoleModel
        {
            SrName = "Operator",
            SrEnName = "Operator",
            SrRemark = "Limited operator role",
            SrReserved1 = string.Empty,
            SrReserved2 = string.Empty,
            SrReserved3 = string.Empty,
            SrStatus = false,
            SrCreateAt = now,
            SrUpdateAt = now
        };

        _context.SysRoles.AddRange(adminRole, operatorRole);
        await _context.SaveChangesAsync(cancellationToken);

        var accounts = new[]
        {
            new SysAccountModel
            {
                SysAccountName = "admin",
                SysPassword = PasswordHasher.HashPassword("123456"),
                SysNickname = "Administrator",
                SysLevel = 0,
                SysRoleId = adminRole.SrId,
                SysStatus = false,
                SysCreateAt = now,
                SysUpdateAt = now
            },
            new SysAccountModel
            {
                SysAccountName = "operator",
                SysPassword = PasswordHasher.HashPassword("123456"),
                SysNickname = "Operator",
                SysLevel = 1,
                SysRoleId = operatorRole.SrId,
                SysStatus = false,
                SysCreateAt = now,
                SysUpdateAt = now
            }
        };

        _context.SysAccounts.AddRange(accounts);
        await _context.SaveChangesAsync(cancellationToken);

        var systemMenu = new SysMenuModel
        {
            SmId = 1,
            SmParentId = 0,
            SmName = "System Management",
            SmEnName = "SystemManagement",
            SmType = 0,
            SmUrl = "#",
            SmTarget = "_self",
            SmLevel = 0,
            SmSort = 1,
            SmIcon = "setting",
            SmRemark = "System module",
            SysStatus = false,
            SysCreateAt = now,
            SysUpdateAt = now
        };

        var userMenu = new SysMenuModel
        {
            SmId = 2,
            SmParentId = 1,
            SmName = "Account Management",
            SmEnName = "UserManagement",
            SmType = 0,
            SmUrl = "/sys/user",
            SmTarget = "_self",
            SmLevel = 1,
            SmSort = 1,
            SmIcon = "user",
            SmRemark = "Account management",
            SysStatus = false,
            SysCreateAt = now,
            SysUpdateAt = now
        };

        var roleMenu = new SysMenuModel
        {
            SmId = 3,
            SmParentId = 1,
            SmName = "Role Management",
            SmEnName = "RoleManagement",
            SmType = 0,
            SmUrl = "/sys/role",
            SmTarget = "_self",
            SmLevel = 1,
            SmSort = 2,
            SmIcon = "team",
            SmRemark = "Role and permission management",
            SysStatus = false,
            SysCreateAt = now,
            SysUpdateAt = now
        };

        var productMenu = new SysMenuModel
        {
            SmId = 4,
            SmParentId = 1,
            SmName = "Product Management",
            SmEnName = "ProductManagement",
            SmType = 0,
            SmUrl = "/template/product",
            SmTarget = "_self",
            SmLevel = 1,
            SmSort = 3,
            SmIcon = "shopping",
            SmRemark = "Template product management sample",
            SysStatus = false,
            SysCreateAt = now,
            SysUpdateAt = now
        };

        var demoMenu = new SysMenuModel
        {
            SmId = 5,
            SmParentId = 0,
            SmName = "Demo",
            SmEnName = "Demo",
            SmType = 0,
            SmUrl = "#",
            SmTarget = "_self",
            SmLevel = 0,
            SmSort = 2,
            SmIcon = "appstore",
            SmRemark = "Data source demo module",
            SysStatus = false,
            SysCreateAt = now,
            SysUpdateAt = now
        };

        var demoEfMenu = new SysMenuModel
        {
            SmId = 6,
            SmParentId = 5,
            SmName = "Demo Notes - EF",
            SmEnName = "DemoNotesEf",
            SmType = 0,
            SmUrl = "/demo/note-ef",
            SmTarget = "_self",
            SmLevel = 1,
            SmSort = 1,
            SmIcon = "database",
            SmRemark = "DemoNote CRUD backed by EF SQLite",
            SysStatus = false,
            SysCreateAt = now,
            SysUpdateAt = now
        };

        var demoApiMenu = new SysMenuModel
        {
            SmId = 7,
            SmParentId = 5,
            SmName = "Demo Notes - WebAPI",
            SmEnName = "DemoNotesWebApi",
            SmType = 0,
            SmUrl = "/demo/note-api",
            SmTarget = "_self",
            SmLevel = 1,
            SmSort = 2,
            SmIcon = "api",
            SmRemark = "DemoNote CRUD backed by WebAPI",
            SysStatus = false,
            SysCreateAt = now,
            SysUpdateAt = now
        };

        var demoLocalMenu = new SysMenuModel
        {
            SmId = 8,
            SmParentId = 5,
            SmName = "Demo Notes - Local",
            SmEnName = "DemoNotesLocal",
            SmType = 0,
            SmUrl = "/demo/note-local",
            SmTarget = "_self",
            SmLevel = 1,
            SmSort = 3,
            SmIcon = "file",
            SmRemark = "DemoNote CRUD backed by local JSON",
            SysStatus = false,
            SysCreateAt = now,
            SysUpdateAt = now
        };

        _context.SysMenus.AddRange(systemMenu, userMenu, roleMenu, productMenu, demoMenu, demoEfMenu, demoApiMenu, demoLocalMenu);
        await _context.SaveChangesAsync(cancellationToken);

        var roleAuths = new[]
        {
            new SysRoleAuthModel { SraRoleId = adminRole.SrId, SraMenuId = systemMenu.SmId },
            new SysRoleAuthModel { SraRoleId = adminRole.SrId, SraMenuId = userMenu.SmId },
            new SysRoleAuthModel { SraRoleId = adminRole.SrId, SraMenuId = roleMenu.SmId },
            new SysRoleAuthModel { SraRoleId = adminRole.SrId, SraMenuId = productMenu.SmId },
            new SysRoleAuthModel { SraRoleId = adminRole.SrId, SraMenuId = demoMenu.SmId },
            new SysRoleAuthModel { SraRoleId = adminRole.SrId, SraMenuId = demoEfMenu.SmId },
            new SysRoleAuthModel { SraRoleId = adminRole.SrId, SraMenuId = demoApiMenu.SmId },
            new SysRoleAuthModel { SraRoleId = adminRole.SrId, SraMenuId = demoLocalMenu.SmId },
            new SysRoleAuthModel { SraRoleId = operatorRole.SrId, SraMenuId = userMenu.SmId }
        };

        _context.SysRoleAuths.AddRange(roleAuths);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
