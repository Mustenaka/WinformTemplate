using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public sealed class ApiSysMenuRepository : ApiRepositoryBase<SysMenuModel>, ISysMenuRepository
{
    public ApiSysMenuRepository(IWebApiClient client) : base(client, "Sys", "menus")
    {
    }

    public async Task<IReadOnlyList<SysMenuModel>> GetActiveMenusAsync()
    {
        return await GetListAsync<SysMenuModel>($"{CollectionEndpoint}/active");
    }

    public async Task<IReadOnlyList<SysMenuModel>> GetByParentIdAsync(long parentId)
    {
        return await GetListAsync<SysMenuModel>($"{CollectionEndpoint}/by-parent/{Escape(parentId)}");
    }

    public async Task FreezeMenuAsync(long id)
    {
        await PostBooleanAsync($"{CollectionEndpoint}/{Escape(id)}/freeze", new { });
    }

    public async Task UnfreezeMenuAsync(long id)
    {
        await PostBooleanAsync($"{CollectionEndpoint}/{Escape(id)}/unfreeze", new { });
    }

    protected override object GetEntityId(SysMenuModel entity)
    {
        return entity.SmId;
    }
}
