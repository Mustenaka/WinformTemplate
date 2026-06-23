using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public sealed class ApiSysAccountRepository : ApiRepositoryBase<SysAccountModel>, ISysAccountRepository
{
    public ApiSysAccountRepository(IWebApiClient client) : base(client, "Sys", "accounts")
    {
    }

    public Task<SysAccountModel?> GetByUsernameAsync(string username)
    {
        return GetOptionalAsync<SysAccountModel>($"{CollectionEndpoint}/by-username/{Escape(username)}");
    }

    public async Task FreezeAccountAsync(long id)
    {
        await PostBooleanAsync($"{CollectionEndpoint}/{Escape(id)}/freeze", new { });
    }

    public async Task UnfreezeAccountAsync(long id)
    {
        await PostBooleanAsync($"{CollectionEndpoint}/{Escape(id)}/unfreeze", new { });
    }

    protected override object GetEntityId(SysAccountModel entity)
    {
        return entity.SysId;
    }
}
