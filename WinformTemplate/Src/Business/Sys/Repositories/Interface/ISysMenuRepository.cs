using WinformTemplate.Business.Sys.Model;

namespace WinformTemplate.Business.Sys.Repositories;

public interface ISysMenuRepository
{
    Task<List<SysMenuModel>> GetAllAsync();
    Task<SysMenuModel?> GetByIdAsync(long id);
    Task AddAsync(SysMenuModel role);
    Task UpdateAsync(SysMenuModel role);
    Task DeleteAsync(long id);
    Task FreezeMenuAsync(long id);
    Task UnfreezeMenuAsync(long id);
}