using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public sealed class ApiSysParamRepository : ApiRepositoryBase<SysParamModel>, ISysParamRepository
{
    public ApiSysParamRepository(IWebApiClient client) : base(client, "Sys", "params")
    {
    }

    public Task<SysParamModel?> GetValueByKey(string key)
    {
        return GetOptionalAsync<SysParamModel>($"{CollectionEndpoint}/by-key/{Escape(key)}");
    }

    public async Task SetValueByKey(string key, string value)
    {
        await PutBooleanAsync($"{CollectionEndpoint}/by-key/{Escape(key)}", new { value });
    }

    protected override object GetEntityId(SysParamModel entity)
    {
        return entity.SpId;
    }
}
