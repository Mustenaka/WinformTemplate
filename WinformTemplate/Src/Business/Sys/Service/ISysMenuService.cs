using WinformTemplate.Business.Sys.Model;

namespace WinformTemplate.Business.Sys.Service;

/// <summary>
/// 菜单服务接口
/// </summary>
public interface ISysMenuService
{
    /// <summary>
    /// 获取所有菜单
    /// </summary>
    Task<IEnumerable<SysMenuModel>> GetAllMenusAsync();

    /// <summary>
    /// 获取菜单树（树形结构）
    /// </summary>
    Task<IEnumerable<SysMenuModel>> GetMenuTreeAsync();

    /// <summary>
    /// 根据角色获取菜单
    /// </summary>
    Task<IEnumerable<SysMenuModel>> GetMenusByRoleIdAsync(long roleId);

    /// <summary>
    /// 根据账户获取菜单（考虑角色权限）
    /// </summary>
    Task<IEnumerable<SysMenuModel>> GetMenusByAccountIdAsync(long accountId);

    /// <summary>
    /// 根据ID获取菜单
    /// </summary>
    Task<SysMenuModel?> GetMenuByIdAsync(long id);

    /// <summary>
    /// 创建菜单
    /// </summary>
    Task<bool> CreateMenuAsync(SysMenuModel menu);

    /// <summary>
    /// 更新菜单
    /// </summary>
    Task<bool> UpdateMenuAsync(SysMenuModel menu);

    /// <summary>
    /// 删除菜单
    /// </summary>
    Task<bool> DeleteMenuAsync(long id);

    /// <summary>
    /// 冻结菜单
    /// </summary>
    Task<bool> FreezeMenuAsync(long id);

    /// <summary>
    /// 解冻菜单
    /// </summary>
    Task<bool> UnfreezeMenuAsync(long id);

    /// <summary>
    /// 获取子菜单
    /// </summary>
    Task<IEnumerable<SysMenuModel>> GetChildMenusAsync(long parentId);
}
