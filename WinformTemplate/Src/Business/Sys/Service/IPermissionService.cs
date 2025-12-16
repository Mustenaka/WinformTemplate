using WinformTemplate.Business.Sys.Model;

namespace WinformTemplate.Business.Sys.Service;

/// <summary>
/// 权限管理服务接口
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// 验证用户是否有指定菜单的权限
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <param name="menuId">菜单ID</param>
    /// <returns>是否有权限</returns>
    Task<bool> HasPermissionAsync(long accountId, long menuId);

    /// <summary>
    /// 获取用户所有可访问的菜单
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <returns>菜单列表</returns>
    Task<IEnumerable<SysMenuModel>> GetAccessibleMenusAsync(long accountId);

    /// <summary>
    /// 获取用户可访问的菜单树（树形结构）
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <returns>菜单树</returns>
    Task<IEnumerable<SysMenuModel>> GetAccessibleMenuTreeAsync(long accountId);

    /// <summary>
    /// 过滤菜单树（根据权限过滤）
    /// </summary>
    /// <param name="allMenus">所有菜单</param>
    /// <param name="accessibleMenuIds">可访问的菜单ID列表</param>
    /// <returns>过滤后的菜单树</returns>
    IEnumerable<SysMenuModel> FilterMenuTree(IEnumerable<SysMenuModel> allMenus, IEnumerable<long> accessibleMenuIds);

    /// <summary>
    /// 检查账户是否有效且未冻结
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <returns>是否有效</returns>
    Task<bool> IsAccountValidAsync(long accountId);

    /// <summary>
    /// 获取角色的所有权限菜单ID
    /// </summary>
    /// <param name="roleId">角色ID</param>
    /// <returns>菜单ID列表</returns>
    Task<IEnumerable<long>> GetRoleMenuIdsAsync(long roleId);
}
