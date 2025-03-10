using WinformTemplate.Business.Sys.Model;

namespace WinformTemplate.Business.Sys.Repositories;

public interface ISysAccountRepository
{
    Task<List<SysAccountModel>> GetAllAsync();
    Task<SysAccountModel?> GetByIdAsync(long id);
    Task AddAsync(SysAccountModel role);
    Task UpdateAsync(SysAccountModel role);
    Task DeleteAsync(long id);
    Task FreezeAccountAsync(long id);
    Task UnfreezeAccountAsync(long id);
}