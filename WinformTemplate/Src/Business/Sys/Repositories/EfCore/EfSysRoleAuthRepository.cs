using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Logger;

namespace WinformTemplate.Business.Sys.Repositories;

public class EfSysRoleAuthRepository : EfRepositoryBase<SysRoleAuthModel>, ISysRoleAuthRepository
{
    public EfSysRoleAuthRepository(SysDbContext context) : base(context)
    {
    }

    public override async Task<SysRoleAuthModel?> GetByIdAsync(object id)
    {
        return id switch
        {
            ValueTuple<long, long> key => await DbSet.FindAsync(key.Item1, key.Item2),
            Tuple<long, long> key => await DbSet.FindAsync(key.Item1, key.Item2),
            object[] { Length: 2 } keys => await DbSet.FindAsync(keys),
            _ => null
        };
    }

    public async Task<IEnumerable<SysRoleAuthModel>> GetByRoleIdAsync(long roleId)
    {
        try
        {
            return await DbSet
                .Where(roleAuth => roleAuth.SraRoleId == roleId)
                .Include(roleAuth => roleAuth.Menu)
                .Include(roleAuth => roleAuth.Role)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Debug.Error($"获取角色权限失败: RoleID={roleId}", ex);
            return Enumerable.Empty<SysRoleAuthModel>();
        }
    }

    public async Task<IEnumerable<SysRoleAuthModel>> GetByMenuIdAsync(long menuId)
    {
        try
        {
            return await DbSet
                .Where(roleAuth => roleAuth.SraMenuId == menuId)
                .Include(roleAuth => roleAuth.Menu)
                .Include(roleAuth => roleAuth.Role)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Debug.Error($"获取菜单权限失败: MenuID={menuId}", ex);
            return Enumerable.Empty<SysRoleAuthModel>();
        }
    }

    public async Task<bool> HasPermissionAsync(long roleId, long menuId)
    {
        try
        {
            return await DbSet.AnyAsync(roleAuth => roleAuth.SraRoleId == roleId && roleAuth.SraMenuId == menuId);
        }
        catch (Exception ex)
        {
            Debug.Error($"检查权限失败: RoleID={roleId}, MenuID={menuId}", ex);
            return false;
        }
    }

    public async Task<bool> AssignPermissionAsync(long roleId, long menuId)
    {
        try
        {
            if (await HasPermissionAsync(roleId, menuId))
            {
                Debug.Warn($"权限已存在: RoleID={roleId}, MenuID={menuId}");
                return true;
            }

            await AddAsync(new SysRoleAuthModel
            {
                SraRoleId = roleId,
                SraMenuId = menuId
            });

            Debug.Info($"分配权限成功: RoleID={roleId}, MenuID={menuId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"分配权限失败: RoleID={roleId}, MenuID={menuId}", ex);
            return false;
        }
    }

    public async Task<bool> RemovePermissionAsync(long roleId, long menuId)
    {
        try
        {
            var roleAuth = await DbSet
                .FirstOrDefaultAsync(item => item.SraRoleId == roleId && item.SraMenuId == menuId);

            if (roleAuth == null)
            {
                Debug.Warn($"权限不存在: RoleID={roleId}, MenuID={menuId}");
                return false;
            }

            DbSet.Remove(roleAuth);
            await DbContext.SaveChangesAsync();

            Debug.Info($"移除权限成功: RoleID={roleId}, MenuID={menuId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"移除权限失败: RoleID={roleId}, MenuID={menuId}", ex);
            return false;
        }
    }

    public async Task<bool> AssignPermissionsBatchAsync(long roleId, IEnumerable<long> menuIds)
    {
        try
        {
            var roleAuths = menuIds.Select(menuId => new SysRoleAuthModel
            {
                SraRoleId = roleId,
                SraMenuId = menuId
            }).ToList();

            await DbSet.AddRangeAsync(roleAuths);
            await DbContext.SaveChangesAsync();

            Debug.Info($"批量分配权限成功: RoleID={roleId}, Count={roleAuths.Count}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"批量分配权限失败: RoleID={roleId}", ex);
            return false;
        }
    }

    public async Task<bool> ClearRolePermissionsAsync(long roleId)
    {
        try
        {
            var roleAuths = await DbSet
                .Where(roleAuth => roleAuth.SraRoleId == roleId)
                .ToListAsync();

            if (!roleAuths.Any())
            {
                return true;
            }

            DbSet.RemoveRange(roleAuths);
            await DbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"清除角色权限失败: RoleID={roleId}", ex);
            return false;
        }
    }
}
