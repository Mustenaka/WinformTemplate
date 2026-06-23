using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

/// <summary>
/// ISysParamRepository
/// </summary>
public interface ISysParamRepository : IRepository<SysParamModel>
{
    Task<SysParamModel?> GetValueByKey(string key);

    Task SetValueByKey(string key, string value);
}
