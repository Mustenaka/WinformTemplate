using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public interface ISysRoleRepository : IRepository<SysRoleModel>
{
    Task<List<SysMenuModel>> GetMenusByRoleIdAsync(long roleId);

    Task<bool> HasMenuPermissionAsync(long roleId, long menuId);

    Task AssignMenuToRoleAsync(long roleId, long menuId);

    Task RemoveMenuFromRoleAsync(long roleId, long menuId);
}
