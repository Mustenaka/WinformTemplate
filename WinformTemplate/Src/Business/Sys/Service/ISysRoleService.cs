using WinformTemplate.Business.Sys.Model;

namespace WinformTemplate.Business.Sys.Service;

/// <summary>
/// 角色服务接口
/// </summary>
public interface ISysRoleService
{
    /// <summary>
    /// 获取所有角色
    /// </summary>
    Task<IEnumerable<SysRoleModel>> GetAllRolesAsync();

    /// <summary>
    /// 根据ID获取角色
    /// </summary>
    Task<SysRoleModel?> GetRoleByIdAsync(long id);

    /// <summary>
    /// 创建角色
    /// </summary>
    Task<bool> CreateRoleAsync(SysRoleModel role);

    /// <summary>
    /// 更新角色
    /// </summary>
    Task<bool> UpdateRoleAsync(SysRoleModel role);

    /// <summary>
    /// 删除角色
    /// </summary>
    Task<bool> DeleteRoleAsync(long id);

    /// <summary>
    /// 分配菜单权限给角色
    /// </summary>
    Task<bool> AssignMenuToRoleAsync(long roleId, long menuId);

    /// <summary>
    /// 批量分配菜单权限给角色
    /// </summary>
    Task<bool> AssignMenusToRoleAsync(long roleId, IEnumerable<long> menuIds);

    /// <summary>
    /// 移除角色的菜单权限
    /// </summary>
    Task<bool> RemoveMenuFromRoleAsync(long roleId, long menuId);

    /// <summary>
    /// 获取角色的所有权限菜单
    /// </summary>
    Task<IEnumerable<SysMenuModel>> GetRoleMenusAsync(long roleId);

    /// <summary>
    /// 检查角色是否有指定菜单权限
    /// </summary>
    Task<bool> HasMenuPermissionAsync(long roleId, long menuId);
}
