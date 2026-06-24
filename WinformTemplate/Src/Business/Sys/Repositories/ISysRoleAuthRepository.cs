using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

/// <summary>
/// 角色权限仓储接口
/// </summary>
public interface ISysRoleAuthRepository : IRepository<SysRoleAuthModel>
{
    /// <summary>
    /// 根据角色ID获取所有权限
    /// </summary>
    Task<IEnumerable<SysRoleAuthModel>> GetByRoleIdAsync(long roleId);

    /// <summary>
    /// 根据菜单ID获取所有角色权限
    /// </summary>
    Task<IEnumerable<SysRoleAuthModel>> GetByMenuIdAsync(long menuId);

    /// <summary>
    /// 检查角色是否有指定菜单权限
    /// </summary>
    Task<bool> HasPermissionAsync(long roleId, long menuId);

    /// <summary>
    /// 为角色分配菜单权限
    /// </summary>
    Task<bool> AssignPermissionAsync(long roleId, long menuId);

    /// <summary>
    /// 移除角色的菜单权限
    /// </summary>
    Task<bool> RemovePermissionAsync(long roleId, long menuId);

    /// <summary>
    /// 批量为角色分配权限
    /// </summary>
    Task<bool> AssignPermissionsBatchAsync(long roleId, IEnumerable<long> menuIds);

    /// <summary>
    /// 清除角色的所有权限
    /// </summary>
    Task<bool> ClearRolePermissionsAsync(long roleId);
}
