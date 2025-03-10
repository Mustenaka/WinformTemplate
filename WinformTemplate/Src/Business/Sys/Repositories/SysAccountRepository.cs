using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.Repository;

namespace WinformTemplate.Business.Sys.Repositories;

/// <summary>
/// 用户信息仓库
/// </summary>
/// <param name="dbContext"></param>
public class SysAccountRepository(SysDbContext dbContext) : BaseRepository<SysAccountModel>(dbContext), ISysAccountRepository
{
    /// <summary>
    /// 获取所有 SysAccountModel
    /// </summary>
    /// <returns></returns>
    public async Task<List<SysAccountModel>> GetAllAsync()
    {
        return await GetAll().ToListAsync();
    }

    /// <summary>
    /// 通过ID获取 SysAccountModel
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SysAccountModel?> GetByIdAsync(long id)
    {
        return await dbContext.SysAccounts.FirstOrDefaultAsync(r => r.SysId == id);
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">用户ID不存在</exception>
    public async Task UpdateAsync(SysAccountModel role)
    {
        // 确保用户存在
        var existingAccount = await dbContext.SysAccounts.FindAsync(role.SysId);
        if (existingAccount == null)
        {
            throw new InvalidOperationException($"用户ID {role.SysId} 不存在");
        }

        // 更新属性
        existingAccount.SysAccountName = role.SysAccountName;
        existingAccount.SysPassword = role.SysPassword;
        existingAccount.SysNickname = role.SysNickname;
        existingAccount.SysLevel = role.SysLevel;
        existingAccount.SysRoleId = role.SysRoleId;
        existingAccount.SysExtendId = role.SysExtendId;
        existingAccount.SysStatus = role.SysStatus;

        existingAccount.SysReserved1 = role.SysReserved1;
        existingAccount.SysReserved2 = role.SysReserved2;
        existingAccount.SysReserved3 = role.SysReserved3;

        existingAccount.SysUpdateAt = DateTime.Now;

        // 更新实体状态
        dbContext.Entry(existingAccount).State = EntityState.Modified;

        // 保存更改
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 表中删除用户（不建议使用此功能）
    /// </summary>
    /// <param name="id">用户id</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task DeleteAsync(long id)
    {
        // 确保用户存在
        var existingAccount = await dbContext.SysAccounts.FindAsync(id);
        if (existingAccount == null)
        {
            throw new InvalidOperationException($"用户ID {id} 不存在");
        }

        // 查找关联的角色权限记录，删除权限组
        var roles = await dbContext.SysRoles.Where(r => r.SrId == existingAccount.SysRoleId).ToListAsync();
        var extends = await dbContext.SysExtends.Where(e => e.SeId == existingAccount.SysExtendId).ToListAsync();

        // 开始关联权限删除
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            // 先删除关联内容
            // 权限组
            if (roles.Any())
            {
                dbContext.SysRoles.RemoveRange(roles);
            }

            // 扩展
            if (extends.Any())
            {
                dbContext.SysExtends.RemoveRange(extends);
            }

            // 再删除角色本身
            dbContext.SysAccounts.Remove(existingAccount);

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
    /// 冻结用户
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task FreezeAccountAsync(long id)
    {
        // 确保用户存在
        var existingAccount = await dbContext.SysAccounts.FindAsync(id);
        if (existingAccount == null)
        {
            throw new InvalidOperationException($"用户ID {id} 不存在");
        }

        // 更新属性
        existingAccount.SysStatus = true;

        existingAccount.SysUpdateAt = DateTime.Now;

        // 更新实体状态
        dbContext.Entry(existingAccount).State = EntityState.Modified;

        // 保存更改
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 解冻用户
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task UnfreezeAccountAsync(long id)
    {
        // 确保用户存在
        var existingAccount = await dbContext.SysAccounts.FindAsync(id);
        if (existingAccount == null)
        {
            throw new InvalidOperationException($"用户ID {id} 不存在");
        }

        // 更新属性
        existingAccount.SysStatus = false;

        existingAccount.SysUpdateAt = DateTime.Now;

        // 更新实体状态
        dbContext.Entry(existingAccount).State = EntityState.Modified;

        // 保存更改
        await dbContext.SaveChangesAsync();
    }
}