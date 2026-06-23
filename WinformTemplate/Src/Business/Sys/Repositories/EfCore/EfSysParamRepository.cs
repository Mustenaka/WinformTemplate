using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public class EfSysParamRepository : EfRepositoryBase<SysParamModel>, ISysParamRepository
{
    public EfSysParamRepository(SysDbContext dbContext) : base(dbContext)
    {
    }

    public Task<SysParamModel?> GetValueByKey(string key)
    {
        return DbSet.FirstOrDefaultAsync(param => param.SpParamKey == key);
    }

    public async Task SetValueByKey(string key, string value)
    {
        var existingParam = await GetValueByKey(key);
        if (existingParam == null)
        {
            await AddAsync(new SysParamModel
            {
                SpParamKey = key,
                SpParamValue = value,
                SpStatus = true,
                SrCreateAt = DateTime.Now,
                SrUpdateAt = DateTime.Now
            });
            return;
        }

        existingParam.SpParamValue = value;
        existingParam.SrUpdateAt = DateTime.Now;
        await DbContext.SaveChangesAsync();
    }

    public override async Task<bool> UpdateAsync(SysParamModel param)
    {
        var existingParam = await DbSet.FindAsync(param.SpId);
        if (existingParam == null)
        {
            return false;
        }

        existingParam.SpParamKey = param.SpParamKey;
        existingParam.SpParamValue = param.SpParamValue;
        existingParam.SpType = param.SpType;
        existingParam.SpSort = param.SpSort;
        existingParam.SpStatus = param.SpStatus;
        existingParam.SrUpdateAt = DateTime.Now;
        existingParam.SrReserved1 = param.SrReserved1;
        existingParam.SrReserved2 = param.SrReserved2;
        existingParam.SysReserved3 = param.SysReserved3;

        return await DbContext.SaveChangesAsync() > 0;
    }

    protected override IQueryable<SysParamModel> ApplyKeyword(IQueryable<SysParamModel> query, string keyword)
    {
        return query.Where(param =>
            param.SpParamKey.Contains(keyword) ||
            param.SpParamValue.Contains(keyword));
    }

    protected override IQueryable<SysParamModel> ApplySorting(IQueryable<SysParamModel> query, QueryRequest req)
    {
        return string.IsNullOrWhiteSpace(req.SortBy)
            ? query.OrderBy(param => param.SpId)
            : base.ApplySorting(query, req);
    }
}
