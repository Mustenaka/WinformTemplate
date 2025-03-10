using WinformTemplate.Business.Sys.Model;

namespace WinformTemplate.Business.Sys.Repositories;

public interface ISysParamRepository
{
    Task<List<SysParamModel>> GetAllAsync();
    Task<SysParamModel?> GetByIdAsync(long id);
    Task<SysParamModel?> GetValueByKey(string key);
    Task SetValueByKey(string key,string value);
    Task AddAsync(SysParamModel role);
    Task UpdateAsync(SysParamModel role);
    Task DeleteAsync(long id);
}