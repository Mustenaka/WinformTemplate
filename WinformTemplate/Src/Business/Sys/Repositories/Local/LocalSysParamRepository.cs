using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public sealed class LocalSysParamRepository : LocalRepositoryBase<SysParamModel>, ISysParamRepository
{
    private const string SeedFile = "sysParams.json";

    public LocalSysParamRepository() : base(SeedFile)
    {
    }

    public LocalSysParamRepository(string seedRoot) : base(SeedFile, seedRoot)
    {
    }

    public Task<SysParamModel?> GetValueByKey(string key)
    {
        return Task.FromResult(Snapshot().FirstOrDefault(param =>
            string.Equals(param.SpParamKey, key, StringComparison.OrdinalIgnoreCase)));
    }

    public Task SetValueByKey(string key, string value)
    {
        Write(items =>
        {
            var param = items.FirstOrDefault(item => string.Equals(item.SpParamKey, key, StringComparison.OrdinalIgnoreCase));
            if (param == null)
            {
                items.Add(new SysParamModel
                {
                    SpId = items.Count == 0 ? 1 : items.Max(item => item.SpId) + 1,
                    SpParamKey = key,
                    SpParamValue = value,
                    SpStatus = false,
                    SrCreateAt = DateTime.Now,
                    SrUpdateAt = DateTime.Now
                });
            }
            else
            {
                param.SpParamValue = value;
                param.SrUpdateAt = DateTime.Now;
            }

            return true;
        });

        return Task.CompletedTask;
    }

    protected override object? GetEntityId(SysParamModel entity)
    {
        return entity.SpId;
    }

    protected override void SetEntityId(SysParamModel entity, long id)
    {
        entity.SpId = id;
    }

    protected override IEnumerable<SysParamModel> ApplyQuery(IEnumerable<SysParamModel> query, QueryRequest req)
    {
        if (!string.IsNullOrWhiteSpace(req.Keyword))
        {
            var keyword = req.Keyword.Trim();
            query = query.Where(param =>
                TextContains(param.SpParamKey, keyword) ||
                TextContains(param.SpParamValue, keyword));
        }

        return query;
    }

    protected override IEnumerable<SysParamModel> ApplySorting(IEnumerable<SysParamModel> query, QueryRequest req)
    {
        return string.IsNullOrWhiteSpace(req.SortBy)
            ? query.OrderBy(param => param.SpSort ?? int.MaxValue).ThenBy(param => param.SpId)
            : base.ApplySorting(query, req);
    }
}
