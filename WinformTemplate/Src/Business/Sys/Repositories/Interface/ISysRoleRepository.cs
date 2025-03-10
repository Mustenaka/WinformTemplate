using WinformTemplate.Business.Sys.Model;

namespace WinformTemplate.Business.Sys.Repositories;

public interface ISysRoleRepository
{
    Task<List<SysRoleModel>> GetAllAsync();
    Task<SysRoleModel?> GetByIdAsync(long id);
    Task<List<SysMenuModel>> GetMenusByRoleIdAsync(long roleId);
    Task AddAsync(SysRoleModel role);
    Task UpdateAsync(SysRoleModel role);
    Task DeleteAsync(long id);
    Task AssignMenuToRoleAsync(long roleId, long menuId);
    Task RemoveMenuFromRoleAsync(long roleId, long menuId);
}