using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.Repository;

namespace WinformTemplate.Business.Sys.Repositories;

/// <summary>
/// 角色权限仓库
/// </summary>
/// <param name="dbContext"></param>
public class SysRoleRepository(SysDbContext dbContext) : BaseRepository<SysRoleModel>(dbContext), ISysRoleRepository
{
    /// <summary>
    /// 获取所有 SysRoleModel
    /// </summary>
    /// <returns></returns>
    public async Task<List<SysRoleModel>> GetAllAsync()
    {
        return await GetAll().ToListAsync();
    }

    /// <summary>
    /// 通过ID获取 SysRoleModel
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SysRoleModel?> GetByIdAsync(long id)
    {
        return await dbContext.SysRoles
            .FirstOrDefaultAsync(r => r.SrId == id);
    }

    /// <summary>
    /// 获取一个角色的所有菜单
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public async Task<List<SysMenuModel>> GetMenusByRoleIdAsync(long roleId)
    {
        return await dbContext.SysRoleAuths
            .Where(ra => ra.SraRoleId == roleId)
            .Select(ra => ra.Menu)
            .ToListAsync();
    }

    /// <summary>
    /// 添加一个角色
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task UpdateAsync(SysRoleModel role)
    {
        // 确保角色存在
        var existingRole = await dbContext.SysRoles.FindAsync(role.SrId);
        if (existingRole == null)
        {
            throw new InvalidOperationException($"角色ID {role.SrId} 不存在");
        }

        // 更新属性
        existingRole.SrName = role.SrName;
        existingRole.SrEnName = role.SrEnName;
        existingRole.SrRemark = role.SrRemark;
        existingRole.SrStatus = role.SrStatus;
        existingRole.SrUpdateAt = DateTime.Now;

        // 更新实体状态
        dbContext.Entry(existingRole).State = EntityState.Modified;

        // 保存更改
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 删除一个菜单及对应的角色授权
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task DeleteAsync(long id)
    {
        // 查找角色
        var role = await dbContext.SysRoles.FindAsync(id);
        if (role == null)
        {
            throw new InvalidOperationException($"角色ID {id} 不存在");
        }

        // 查找关联的角色权限记录
        var roleAuths = await dbContext.SysRoleAuths
            .Where(ra => ra.SraRoleId == id)
            .ToListAsync();

        // 开始事务
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            // 先删除角色权限关联
            if (roleAuths.Any())
            {
                dbContext.SysRoleAuths.RemoveRange(roleAuths);
            }

            // 再删除角色
            dbContext.SysRoles.Remove(role);

            // 保存更改
            await dbContext.SaveChangesAsync();

            // 提交事务
            await transaction.CommitAsync();
        }
        catch
        {
            // 回滚事务
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// 分配菜单给角色
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="menuId"></param>
    /// <returns></returns>
    public async Task AssignMenuToRoleAsync(long roleId, long menuId)
    {
        var roleAuth = new SysRoleAuthModel
        {
            SraRoleId = roleId,
            SraMenuId = menuId
        };

        await dbContext.SysRoleAuths.AddAsync(roleAuth);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 从角色中移除菜单
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="menuId"></param>
    /// <returns></returns>
    public async Task RemoveMenuFromRoleAsync(long roleId, long menuId)
    {
        var roleAuth = await dbContext.SysRoleAuths
            .FirstOrDefaultAsync(ra => ra.SraRoleId == roleId && ra.SraMenuId == menuId);

        if (roleAuth != null)
        {
            dbContext.SysRoleAuths.Remove(roleAuth);
            await dbContext.SaveChangesAsync();
        }
    }
}