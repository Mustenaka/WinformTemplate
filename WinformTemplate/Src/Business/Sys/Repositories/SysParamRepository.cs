using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.Repository;

namespace WinformTemplate.Business.Sys.Repositories;

/// <summary>
/// 系统参数仓库
/// </summary>
/// <param name="dbContext"></param>
public class SysParamRepository(SysDbContext dbContext) : BaseRepository<SysParamModel>(dbContext), ISysParamRepository
{
    /// <summary>
    /// 获取所有 SysParamModel
    /// </summary>
    /// <returns></returns>
    public async Task<List<SysParamModel>> GetAllAsync()
    {
        return await GetAll().ToListAsync();
    }

    /// <summary>
    /// 通过ID获取 SysParamModel
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SysParamModel?> GetByIdAsync(long id)
    {
        return await dbContext.SysParams.FirstOrDefaultAsync(p => p.SpId == id);
    }

    /// <summary>
    /// 通过Key获取 SysParamModel
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<SysParamModel?> GetValueByKey(string key)
    {
        return await dbContext.SysParams.FirstOrDefaultAsync(p => p.SpParamKey == key);
    }

    /// <summary>
    /// 更新系统参数, 如果不存在这个参数则创建
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task SetValueByKey(string key, string value)
    {
        var existingParam = await dbContext.SysParams.FirstOrDefaultAsync(p => p.SpParamKey == key);
        if (existingParam == null)
        {
            existingParam = new SysParamModel
            {
                SpParamKey = key,
                SpParamValue = value,
                SpStatus = true,
                SrCreateAt = DateTime.Now,
                SrUpdateAt = DateTime.Now
            };

            await dbContext.SysParams.AddAsync(existingParam);
            return;
        }

        // 更新具体参数
        existingParam.SpParamValue = value;

        // 更新实体状态
        dbContext.Entry(existingParam).State = EntityState.Modified;

        // 保存更改
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(SysParamModel role)
    {
        var existingParam = await dbContext.SysParams.FindAsync(role.SpId);
        if (existingParam == null)
        {
            throw new InvalidOperationException($"菜单ID {role.SpId} 不存在");
        }

        // 更新具体参数
        existingParam.SpParamKey = role.SpParamKey;
        existingParam.SpParamValue = role.SpParamValue;
        existingParam.SpType = role.SpType;
        existingParam.SpSort = role.SpSort;
        existingParam.SpStatus = role.SpStatus;

        existingParam.SrUpdateAt = DateTime.Now;

        existingParam.SrReserved1 = role.SrReserved1;
        existingParam.SrReserved2 = role.SrReserved2;
        existingParam.SysReserved3 = role.SysReserved3;

        // 更新实体状态
        dbContext.Entry(existingParam).State = EntityState.Modified;

        // 保存更改
        await dbContext.SaveChangesAsync();
    }

    public Task DeleteAsync(long id)
    {
        throw new NotImplementedException();
    }
}