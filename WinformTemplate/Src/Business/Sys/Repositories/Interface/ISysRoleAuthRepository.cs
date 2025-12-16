using WinformTemplate.Business.Sys.Model;

namespace WinformTemplate.Business.Sys.Repositories;

/// <summary>
/// 角色权限仓储接口
/// </summary>
public interface ISysRoleAuthRepository
{
    /// <summary>
    /// 根据角色ID获取所有权限
    /// </summary>
    Task<IEnumerable<SysRoleAuthModel>> GetByRoleIdAsync(int roleId);

    /// <summary>
    /// 根据菜单ID获取所有角色权限
    /// </summary>
    Task<IEnumerable<SysRoleAuthModel>> GetByMenuIdAsync(int menuId);

    /// <summary>
    /// 检查角色是否有指定菜单权限
    /// </summary>
    Task<bool> HasPermissionAsync(int roleId, int menuId);

    /// <summary>
    /// 为角色分配菜单权限
    /// </summary>
    Task<bool> AssignPermissionAsync(int roleId, int menuId);

    /// <summary>
    /// 移除角色的菜单权限
    /// </summary>
    Task<bool> RemovePermissionAsync(int roleId, int menuId);

    /// <summary>
    /// 批量为角色分配权限
    /// </summary>
    Task<bool> AssignPermissionsBatchAsync(int roleId, IEnumerable<int> menuIds);

    /// <summary>
    /// 清除角色的所有权限
    /// </summary>
    Task<bool> ClearRolePermissionsAsync(int roleId);
}
