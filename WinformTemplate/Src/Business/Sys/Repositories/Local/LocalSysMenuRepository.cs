using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public sealed class LocalSysMenuRepository : LocalRepositoryBase<SysMenuModel>, ISysMenuRepository
{
    private const string SeedFile = "sysMenus.json";

    public LocalSysMenuRepository() : base(SeedFile)
    {
    }

    public LocalSysMenuRepository(string seedRoot) : base(SeedFile, seedRoot)
    {
    }

    public Task<IReadOnlyList<SysMenuModel>> GetActiveMenusAsync()
    {
        return Task.FromResult<IReadOnlyList<SysMenuModel>>(Snapshot()
            .Where(menu => menu.SysStatus == false)
            .OrderBy(menu => menu.SmSort ?? int.MaxValue)
            .ThenBy(menu => menu.SmId)
            .ToList());
    }

    public Task<IReadOnlyList<SysMenuModel>> GetByParentIdAsync(long parentId)
    {
        return Task.FromResult<IReadOnlyList<SysMenuModel>>(Snapshot()
            .Where(menu => menu.SmParentId == parentId)
            .OrderBy(menu => menu.SmSort ?? int.MaxValue)
            .ThenBy(menu => menu.SmId)
            .ToList());
    }

    public Task FreezeMenuAsync(long id)
    {
        SetMenuStatus(id, true);
        return Task.CompletedTask;
    }

    public Task UnfreezeMenuAsync(long id)
    {
        SetMenuStatus(id, false);
        return Task.CompletedTask;
    }

    protected override object? GetEntityId(SysMenuModel entity)
    {
        return entity.SmId;
    }

    protected override void SetEntityId(SysMenuModel entity, long id)
    {
        entity.SmId = id;
    }

    protected override IEnumerable<SysMenuModel> ApplyQuery(IEnumerable<SysMenuModel> query, QueryRequest req)
    {
        if (!string.IsNullOrWhiteSpace(req.Keyword))
        {
            var keyword = req.Keyword.Trim();
            query = query.Where(menu =>
                TextContains(menu.SmName, keyword) ||
                TextContains(menu.SmEnName, keyword) ||
                TextContains(menu.SmRemark, keyword));
        }

        var parentId = TryLong(req.Filters, "parentId");
        if (parentId.HasValue)
        {
            query = query.Where(menu => menu.SmParentId == parentId.Value);
        }

        var status = TryBool(req.Filters, "status");
        if (status.HasValue)
        {
            query = query.Where(menu => menu.SysStatus == status.Value);
        }

        return query;
    }

    protected override IEnumerable<SysMenuModel> ApplySorting(IEnumerable<SysMenuModel> query, QueryRequest req)
    {
        return string.IsNullOrWhiteSpace(req.SortBy)
            ? query.OrderBy(menu => menu.SmSort ?? int.MaxValue).ThenBy(menu => menu.SmId)
            : base.ApplySorting(query, req);
    }

    private void SetMenuStatus(long id, bool status)
    {
        var changed = Write(items =>
        {
            var menu = items.FirstOrDefault(item => item.SmId == id);
            if (menu == null)
            {
                return false;
            }

            menu.SysStatus = status;
            menu.SysUpdateAt = DateTime.Now;
            return true;
        });

        if (!changed)
        {
            throw new InvalidOperationException($"Menu {id} does not exist.");
        }
    }

    private static bool? TryBool(IReadOnlyDictionary<string, string>? filters, string key)
    {
        return filters != null && filters.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed)
            ? parsed
            : null;
    }
}
