using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.Repository;
using WinformTemplate.Logger;

namespace WinformTemplate.Business.Sys.Repositories;

/// <summary>
/// 角色权限仓储实现
/// </summary>
public class SysRoleAuthRepository : BaseRepository<SysRoleAuthModel>, ISysRoleAuthRepository
{
    public SysRoleAuthRepository(SysDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 根据角色ID获取所有权限
    /// </summary>
    public async Task<IEnumerable<SysRoleAuthModel>> GetByRoleIdAsync(int roleId)
    {
        try
        {
            return await dbSet
                .Where(ra => ra.SraRoleId == roleId)
                .Include(ra => ra.Menu)
                .Include(ra => ra.Role)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Debug.Error($"获取角色权限失败: RoleID={roleId}", ex);
            return Enumerable.Empty<SysRoleAuthModel>();
        }
    }

    /// <summary>
    /// 根据菜单ID获取所有角色权限
    /// </summary>
    public async Task<IEnumerable<SysRoleAuthModel>> GetByMenuIdAsync(int menuId)
    {
        try
        {
            return await dbSet
                .Where(ra => ra.SraMenuId == menuId)
                .Include(ra => ra.Menu)
                .Include(ra => ra.Role)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Debug.Error($"获取菜单权限失败: MenuID={menuId}", ex);
            return Enumerable.Empty<SysRoleAuthModel>();
        }
    }

    /// <summary>
    /// 检查角色是否有指定菜单权限
    /// </summary>
    public async Task<bool> HasPermissionAsync(int roleId, int menuId)
    {
        try
        {
            return await dbSet.AnyAsync(ra => ra.SraRoleId == roleId && ra.SraMenuId == menuId);
        }
        catch (Exception ex)
        {
            Debug.Error($"检查权限失败: RoleID={roleId}, MenuID={menuId}", ex);
            return false;
        }
    }

    /// <summary>
    /// 为角色分配菜单权限
    /// </summary>
    public async Task<bool> AssignPermissionAsync(int roleId, int menuId)
    {
        try
        {
            // 检查是否已存在
            var exists = await HasPermissionAsync(roleId, menuId);
            if (exists)
            {
                Debug.Warn($"权限已存在: RoleID={roleId}, MenuID={menuId}");
                return true;
            }

            var roleAuth = new SysRoleAuthModel
            {
                SraRoleId = roleId,
                SraMenuId = menuId
            };

            await AddAsync(roleAuth);
            await SaveChangesAsync();

            Debug.Info($"分配权限成功: RoleID={roleId}, MenuID={menuId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"分配权限失败: RoleID={roleId}, MenuID={menuId}", ex);
            return false;
        }
    }

    /// <summary>
    /// 移除角色的菜单权限
    /// </summary>
    public async Task<bool> RemovePermissionAsync(int roleId, int menuId)
    {
        try
        {
            var roleAuth = await dbSet
                .FirstOrDefaultAsync(ra => ra.SraRoleId == roleId && ra.SraMenuId == menuId);

            if (roleAuth == null)
            {
                Debug.Warn($"权限不存在: RoleID={roleId}, MenuID={menuId}");
                return false;
            }

            Delete(roleAuth);
            await SaveChangesAsync();

            Debug.Info($"移除权限成功: RoleID={roleId}, MenuID={menuId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"移除权限失败: RoleID={roleId}, MenuID={menuId}", ex);
            return false;
        }
    }

    /// <summary>
    /// 批量为角色分配权限
    /// </summary>
    public async Task<bool> AssignPermissionsBatchAsync(int roleId, IEnumerable<int> menuIds)
    {
        try
        {
            Debug.Info($"批量分配权限: RoleID={roleId}, MenuCount={menuIds.Count()}");

            var roleAuths = menuIds.Select(menuId => new SysRoleAuthModel
            {
                SraRoleId = roleId,
                SraMenuId = menuId
            }).ToList();

            await AddRangeAsync(roleAuths);
            await SaveChangesAsync();

            Debug.Info($"批量分配权限成功: RoleID={roleId}, Count={roleAuths.Count}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"批量分配权限失败: RoleID={roleId}", ex);
            return false;
        }
    }

    /// <summary>
    /// 清除角色的所有权限
    /// </summary>
    public async Task<bool> ClearRolePermissionsAsync(int roleId)
    {
        try
        {
            Debug.Info($"清除角色权限: RoleID={roleId}");

            var roleAuths = await dbSet
                .Where(ra => ra.SraRoleId == roleId)
                .ToListAsync();

            if (!roleAuths.Any())
            {
                Debug.Info($"角色无权限可清除: RoleID={roleId}");
                return true;
            }

            DeleteRange(roleAuths);
            await SaveChangesAsync();

            Debug.Info($"清除角色权限成功: RoleID={roleId}, Count={roleAuths.Count}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"清除角色权限失败: RoleID={roleId}", ex);
            return false;
        }
    }
}
