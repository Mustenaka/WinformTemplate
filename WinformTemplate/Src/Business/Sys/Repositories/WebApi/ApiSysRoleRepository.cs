using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public sealed class ApiSysRoleRepository : ApiRepositoryBase<SysRoleModel>, ISysRoleRepository
{
    public ApiSysRoleRepository(IWebApiClient client) : base(client, "Sys", "roles")
    {
    }

    public async Task<List<SysMenuModel>> GetMenusByRoleIdAsync(long roleId)
    {
        return (await GetListAsync<SysMenuModel>($"{CollectionEndpoint}/{Escape(roleId)}/menus")).ToList();
    }

    public Task<bool> HasMenuPermissionAsync(long roleId, long menuId)
    {
        return GetBooleanAsync($"{CollectionEndpoint}/{Escape(roleId)}/menus/{Escape(menuId)}/exists");
    }

    public async Task AssignMenuToRoleAsync(long roleId, long menuId)
    {
        await PostBooleanAsync($"{CollectionEndpoint}/{Escape(roleId)}/menus", new { menuId });
    }

    public async Task RemoveMenuFromRoleAsync(long roleId, long menuId)
    {
        await DeleteBooleanAsync($"{CollectionEndpoint}/{Escape(roleId)}/menus/{Escape(menuId)}");
    }

    protected override object GetEntityId(SysRoleModel entity)
    {
        return entity.SrId;
    }
}
