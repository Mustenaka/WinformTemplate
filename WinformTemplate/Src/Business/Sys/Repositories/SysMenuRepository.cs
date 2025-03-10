using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.Repository;

namespace WinformTemplate.Business.Sys.Repositories;

/// <summary>
/// 菜单权限仓库
/// </summary>
/// <param name="dbContext"></param>
public class SysMenuRepository(SysDbContext dbContext) : BaseRepository<SysMenuModel>(dbContext), ISysMenuRepository
{
    /// <summary>
    /// 获取所有 SysMenuModel
    /// </summary>
    /// <returns></returns>
    public async Task<List<SysMenuModel>> GetAllAsync()
    {
        return await GetAll().ToListAsync();
    }

    /// <summary>
    /// 通过ID获取 SysMenuModel
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SysMenuModel?> GetByIdAsync(long id)
    {
        return await dbContext.SysMenus.FirstOrDefaultAsync(m => m.SmId == id);
    }

    /// <summary>
    /// 更新菜单
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task UpdateAsync(SysMenuModel role)
    {
        var existingMenu = await dbContext.SysMenus.FindAsync(role.SmId);
        if (existingMenu == null)
        {
            throw new InvalidOperationException($"菜单ID {role.SmId} 不存在");
        }

        // 更新属性
        existingMenu.SmParentId = role.SmParentId;
        existingMenu.SmName = role.SmName;
        existingMenu.SmEnName = role.SmEnName;
        existingMenu.SmType = role.SmType;
        existingMenu.SmUrl = role.SmUrl;
        existingMenu.SmTarget = role.SmTarget;
        existingMenu.SmLevel = role.SmLevel;
        existingMenu.SmSort = role.SmSort;
        existingMenu.SmIcon = role.SmIcon;
        existingMenu.SmRemark = role.SmRemark;
        existingMenu.SysStatus = role.SysStatus;

        existingMenu.SysUpdateAt = DateTime.Now;

        existingMenu.SysReserved1 = role.SysReserved1;
        existingMenu.SysReserved2 = role.SysReserved2;
        existingMenu.SysReserved3 = role.SysReserved3;

        // 更新实体状态
        dbContext.Entry(existingMenu).State = EntityState.Modified;

        // 保存更改
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 删除菜单
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task DeleteAsync(long id)
    {
        var existingMenu = await dbContext.SysMenus.FindAsync(id);
        if (existingMenu == null)
        {
            throw new InvalidOperationException($"菜单ID {id} 不存在");
        }

        // 查找关联的role auth权限记录，删除权限组
        var roleAuths = await dbContext.SysRoleAuths.Where(ra => ra.SraMenuId == id).ToListAsync();

        // 开始事务
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            // 先删除角色权限关联
            if (roleAuths.Any())
            {
                dbContext.SysRoleAuths.RemoveRange(roleAuths);
            }

            // 再删除菜单
            dbContext.SysMenus.Remove(existingMenu);

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

    public async Task FreezeMenuAsync(long id)
    {
        // 确保用户存在
        var existingMenu = await dbContext.SysMenus.FindAsync(id);
        if (existingMenu == null)
        {
            throw new InvalidOperationException($"用户ID {id} 不存在");
        }

        // 更新属性
        existingMenu.SysStatus = true;

        existingMenu.SysUpdateAt = DateTime.Now;

        // 更新实体状态
        dbContext.Entry(existingMenu).State = EntityState.Modified;

        // 保存更改
        await dbContext.SaveChangesAsync();
    }

    public async Task UnfreezeMenuAsync(long id)
    {
        // 确保用户存在
        var existingMenu = await dbContext.SysMenus.FindAsync(id);
        if (existingMenu == null)
        {
            throw new InvalidOperationException($"用户ID {id} 不存在");
        }

        // 更新属性
        existingMenu.SysStatus = false;

        existingMenu.SysUpdateAt = DateTime.Now;

        // 更新实体状态
        dbContext.Entry(existingMenu).State = EntityState.Modified;

        // 保存更改
        await dbContext.SaveChangesAsync();
    }
}