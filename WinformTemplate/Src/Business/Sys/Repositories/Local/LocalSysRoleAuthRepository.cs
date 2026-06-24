using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public sealed class LocalSysRoleAuthRepository : LocalRepositoryBase<SysRoleAuthModel>, ISysRoleAuthRepository
{
    private const string SeedFile = "sysRoleAuths.json";

    public LocalSysRoleAuthRepository() : base(SeedFile)
    {
    }

    public LocalSysRoleAuthRepository(string seedRoot) : base(SeedFile, seedRoot)
    {
    }

    public Task<IEnumerable<SysRoleAuthModel>> GetByRoleIdAsync(long roleId)
    {
        return Task.FromResult<IEnumerable<SysRoleAuthModel>>(Snapshot()
            .Where(roleAuth => roleAuth.SraRoleId == roleId)
            .ToList());
    }

    public Task<IEnumerable<SysRoleAuthModel>> GetByMenuIdAsync(long menuId)
    {
        return Task.FromResult<IEnumerable<SysRoleAuthModel>>(Snapshot()
            .Where(roleAuth => roleAuth.SraMenuId == menuId)
            .ToList());
    }

    public Task<bool> HasPermissionAsync(long roleId, long menuId)
    {
        return Task.FromResult(Snapshot().Any(roleAuth =>
            roleAuth.SraRoleId == roleId &&
            roleAuth.SraMenuId == menuId));
    }

    public Task<bool> AssignPermissionAsync(long roleId, long menuId)
    {
        return Task.FromResult(Write(items =>
        {
            if (items.Any(roleAuth => roleAuth.SraRoleId == roleId && roleAuth.SraMenuId == menuId))
            {
                return true;
            }

            items.Add(new SysRoleAuthModel
            {
                SraRoleId = roleId,
                SraMenuId = menuId
            });
            return true;
        }));
    }

    public Task<bool> RemovePermissionAsync(long roleId, long menuId)
    {
        return Task.FromResult(Write(items =>
            items.RemoveAll(roleAuth => roleAuth.SraRoleId == roleId && roleAuth.SraMenuId == menuId) > 0));
    }

    public async Task<bool> AssignPermissionsBatchAsync(long roleId, IEnumerable<long> menuIds)
    {
        foreach (var menuId in menuIds)
        {
            await AssignPermissionAsync(roleId, menuId);
        }

        return true;
    }

    public Task<bool> ClearRolePermissionsAsync(long roleId)
    {
        return Task.FromResult(Write(items =>
        {
            items.RemoveAll(roleAuth => roleAuth.SraRoleId == roleId);
            return true;
        }));
    }

    protected override object? GetEntityId(SysRoleAuthModel entity)
    {
        return $"{entity.SraRoleId}:{entity.SraMenuId}";
    }

    protected override IEnumerable<SysRoleAuthModel> ApplyQuery(IEnumerable<SysRoleAuthModel> query, QueryRequest req)
    {
        var roleId = TryLong(req.Filters, "roleId");
        if (roleId.HasValue)
        {
            query = query.Where(roleAuth => roleAuth.SraRoleId == roleId.Value);
        }

        var menuId = TryLong(req.Filters, "menuId");
        if (menuId.HasValue)
        {
            query = query.Where(roleAuth => roleAuth.SraMenuId == menuId.Value);
        }

        return query;
    }
}
