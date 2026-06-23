using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public sealed class ApiSysRoleAuthRepository : ApiRepositoryBase<SysRoleAuthModel>, ISysRoleAuthRepository
{
    public ApiSysRoleAuthRepository(IWebApiClient client) : base(client, "Sys", "role-auths")
    {
    }

    public async Task<IEnumerable<SysRoleAuthModel>> GetByRoleIdAsync(long roleId)
    {
        return await GetListAsync<SysRoleAuthModel>($"{CollectionEndpoint}/by-role/{Escape(roleId)}");
    }

    public async Task<IEnumerable<SysRoleAuthModel>> GetByMenuIdAsync(long menuId)
    {
        return await GetListAsync<SysRoleAuthModel>($"{CollectionEndpoint}/by-menu/{Escape(menuId)}");
    }

    public Task<bool> HasPermissionAsync(long roleId, long menuId)
    {
        return GetBooleanAsync($"{CollectionEndpoint}/{Escape(roleId)}/{Escape(menuId)}/exists");
    }

    public Task<bool> AssignPermissionAsync(long roleId, long menuId)
    {
        return PostBooleanAsync(CollectionEndpoint, new { roleId, menuId });
    }

    public Task<bool> RemovePermissionAsync(long roleId, long menuId)
    {
        return DeleteBooleanAsync($"{CollectionEndpoint}/{Escape(roleId)}/{Escape(menuId)}");
    }

    public Task<bool> AssignPermissionsBatchAsync(long roleId, IEnumerable<long> menuIds)
    {
        return PostBooleanAsync($"{CollectionEndpoint}/batch", new { roleId, menuIds = menuIds.ToArray() });
    }

    public Task<bool> ClearRolePermissionsAsync(long roleId)
    {
        return DeleteBooleanAsync($"{CollectionEndpoint}/by-role/{Escape(roleId)}");
    }

    protected override object GetEntityId(SysRoleAuthModel entity)
    {
        return $"{entity.SraRoleId}:{entity.SraMenuId}";
    }
}
