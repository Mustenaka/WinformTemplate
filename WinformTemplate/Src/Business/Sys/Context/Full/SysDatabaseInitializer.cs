using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Sys.Context;
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

        var adminRole = new SysRoleModel
        {
            SrName = "管理员",
            SrEnName = "Admin",
            SrRemark = "系统管理员角色",
            SrReserved1 = string.Empty,
            SrReserved2 = string.Empty,
            SysReserved3 = string.Empty,
            SrStatus = false,
            SrCreateAt = DateTime.Now
        };

        _context.SysRoles.Add(adminRole);
        await _context.SaveChangesAsync(cancellationToken);

        var adminAccount = new SysAccountModel
        {
            SysAccountName = "admin",
            SysPassword = PasswordHasher.HashPassword("123456"),
            SysNickname = "系统管理员",
            SysLevel = 0,
            SysRoleId = adminRole.SrId,
            SysStatus = false,
            SysCreateAt = DateTime.Now
        };

        _context.SysAccounts.Add(adminAccount);
        await _context.SaveChangesAsync(cancellationToken);

        var systemMenu = new SysMenuModel
        {
            SmId = 1,
            SmParentId = 0,
            SmName = "系统管理",
            SmEnName = "SystemManagement",
            SmType = 0,
            SmUrl = "#",
            SmTarget = "_self",
            SmLevel = 0,
            SmSort = 1,
            SmIcon = "setting",
            SmRemark = "系统管理菜单",
            SysStatus = false,
            SysCreateAt = DateTime.Now
        };

        var userMenu = new SysMenuModel
        {
            SmId = 2,
            SmParentId = 1,
            SmName = "用户管理",
            SmEnName = "UserManagement",
            SmType = 0,
            SmUrl = "/sys/user",
            SmTarget = "_self",
            SmLevel = 1,
            SmSort = 1,
            SmIcon = "user",
            SmRemark = "用户账户管理",
            SysStatus = false,
            SysCreateAt = DateTime.Now
        };

        var roleMenu = new SysMenuModel
        {
            SmId = 3,
            SmParentId = 1,
            SmName = "角色管理",
            SmEnName = "RoleManagement",
            SmType = 0,
            SmUrl = "/sys/role",
            SmTarget = "_self",
            SmLevel = 1,
            SmSort = 2,
            SmIcon = "team",
            SmRemark = "角色权限管理",
            SysStatus = false,
            SysCreateAt = DateTime.Now
        };

        _context.SysMenus.AddRange(systemMenu, userMenu, roleMenu);
        await _context.SaveChangesAsync(cancellationToken);

        var roleAuths = new[]
        {
            new SysRoleAuthModel { SraRoleId = adminRole.SrId, SraMenuId = systemMenu.SmId },
            new SysRoleAuthModel { SraRoleId = adminRole.SrId, SraMenuId = userMenu.SmId },
            new SysRoleAuthModel { SraRoleId = adminRole.SrId, SraMenuId = roleMenu.SmId }
        };

        _context.SysRoleAuths.AddRange(roleAuths);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
