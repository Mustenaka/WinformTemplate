using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.Repositories;
using Debug = WinformTemplate.Logger.Debug;

namespace WinformTemplate.Business.Sys.Service;

/// <summary>
/// 权限管理服务实现
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly ISysAccountRepository _accountRepository;
    private readonly ISysRoleRepository _roleRepository;
    private readonly ISysMenuRepository _menuRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PermissionService(
        ISysAccountRepository accountRepository,
        ISysRoleRepository roleRepository,
        ISysMenuRepository menuRepository)
    {
        _accountRepository = accountRepository;
        _roleRepository = roleRepository;
        _menuRepository = menuRepository;
    }

    /// <summary>
    /// 验证用户是否有指定菜单的权限
    /// </summary>
    public async Task<bool> HasPermissionAsync(long accountId, long menuId)
    {
        try
        {
            Debug.Info($"验证用户权限，账户ID：{accountId}，菜单ID：{menuId}");

            // 检查账户是否有效
            if (!await IsAccountValidAsync(accountId))
            {
                Debug.Warn($"验证权限失败：账户无效或已冻结，ID：{accountId}");
                return false;
            }

            // 获取账户信息
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account?.SysRoleId == null)
            {
                Debug.Warn($"验证权限失败：账户没有分配角色，ID：{accountId}");
                return false;
            }

            // 获取角色的所有菜单权限
            var roleMenus = await _roleRepository.GetMenusByRoleIdAsync(account.SysRoleId.Value);
            var hasPermission = roleMenus.Any(m => m.SmId == menuId && m.SysStatus == false);

            Debug.Info($"权限验证结果：{hasPermission}，账户ID：{accountId}，菜单ID：{menuId}");
            return hasPermission;
        }
        catch (Exception ex)
        {
            Debug.Error($"验证用户权限异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 获取用户所有可访问的菜单
    /// </summary>
    public async Task<IEnumerable<SysMenuModel>> GetAccessibleMenusAsync(long accountId)
    {
        try
        {
            Debug.Info($"获取用户可访问菜单，账户ID：{accountId}");

            // 检查账户是否有效
            if (!await IsAccountValidAsync(accountId))
            {
                Debug.Warn($"获取可访问菜单失败：账户无效或已冻结，ID：{accountId}");
                return Enumerable.Empty<SysMenuModel>();
            }

            // 获取账户信息
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account?.SysRoleId == null)
            {
                Debug.Warn($"获取可访问菜单：账户没有分配角色，ID：{accountId}");
                return Enumerable.Empty<SysMenuModel>();
            }

            // 获取角色的所有菜单权限
            var menus = await _roleRepository.GetMenusByRoleIdAsync(account.SysRoleId.Value);

            // 过滤有效菜单并排序
            var validMenus = menus.Where(m => m.SysStatus == false)
                                  .OrderBy(m => m.SmSort ?? int.MaxValue)
                                  .ToList();

            Debug.Info($"用户可访问菜单获取成功，账户ID：{accountId}，菜单数量：{validMenus.Count}");
            return validMenus;
        }
        catch (Exception ex)
        {
            Debug.Error($"获取用户可访问菜单异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 获取用户可访问的菜单树（树形结构）
    /// </summary>
    public async Task<IEnumerable<SysMenuModel>> GetAccessibleMenuTreeAsync(long accountId)
    {
        try
        {
            Debug.Info($"获取用户可访问菜单树，账户ID：{accountId}");

            // 获取用户可访问的所有菜单
            var accessibleMenus = await GetAccessibleMenusAsync(accountId);
            if (!accessibleMenus.Any())
            {
                return Enumerable.Empty<SysMenuModel>();
            }

            // 获取所有菜单
            var allMenus = await _menuRepository.GetAllAsync();

            // 获取可访问的菜单ID列表
            var accessibleMenuIds = accessibleMenus.Select(m => m.SmId).ToList();

            // 过滤菜单树
            var menuTree = FilterMenuTree(allMenus, accessibleMenuIds);

            Debug.Info($"用户可访问菜单树获取成功，账户ID：{accountId}");
            return menuTree;
        }
        catch (Exception ex)
        {
            Debug.Error($"获取用户可访问菜单树异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 过滤菜单树（根据权限过滤）
    /// </summary>
    public IEnumerable<SysMenuModel> FilterMenuTree(IEnumerable<SysMenuModel> allMenus, IEnumerable<long> accessibleMenuIds)
    {
        try
        {
            Debug.Info($"过滤菜单树，可访问菜单数量：{accessibleMenuIds.Count()}");

            var accessibleMenuIdSet = new HashSet<long>(accessibleMenuIds);
            var result = new List<SysMenuModel>();

            // 首先找出所有可访问的菜单
            var accessibleMenuList = allMenus.Where(m => accessibleMenuIdSet.Contains(m.SmId) && m.SysStatus == false).ToList();

            // 为每个可访问的菜单，确保其父菜单路径也包含在内（即使父菜单不在权限列表中）
            var menuIdToInclude = new HashSet<long>(accessibleMenuIdSet);
            foreach (var menu in accessibleMenuList)
            {
                var parentId = menu.SmParentId;
                while (parentId > 0)
                {
                    if (menuIdToInclude.Contains(parentId))
                        break;

                    menuIdToInclude.Add(parentId);
                    var parentMenu = allMenus.FirstOrDefault(m => m.SmId == parentId);
                    if (parentMenu == null)
                        break;

                    parentId = parentMenu.SmParentId;
                }
            }

            // 返回需要包含的所有菜单，按排序规则排序
            result = allMenus.Where(m => menuIdToInclude.Contains(m.SmId) && m.SysStatus == false)
                            .OrderBy(m => m.SmSort ?? int.MaxValue)
                            .ToList();

            Debug.Info($"菜单树过滤完成，结果数量：{result.Count}");
            return result;
        }
        catch (Exception ex)
        {
            Debug.Error($"过滤菜单树异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 检查账户是否有效且未冻结
    /// </summary>
    public async Task<bool> IsAccountValidAsync(long accountId)
    {
        try
        {
            Debug.Info($"检查账户是否有效，账户ID：{accountId}");

            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                Debug.Warn($"账户不存在，ID：{accountId}");
                return false;
            }

            // SysStatus=false表示有效，true表示无效/冻结
            var isValid = account.SysStatus == false;
            Debug.Info($"账户有效性检查结果：{isValid}，账户ID：{accountId}");
            return isValid;
        }
        catch (Exception ex)
        {
            Debug.Error($"检查账户有效性异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 获取角色的所有权限菜单ID
    /// </summary>
    public async Task<IEnumerable<long>> GetRoleMenuIdsAsync(long roleId)
    {
        try
        {
            Debug.Info($"获取角色权限菜单ID，角色ID：{roleId}");

            var menus = await _roleRepository.GetMenusByRoleIdAsync(roleId);
            var menuIds = menus.Where(m => m.SysStatus == false)
                              .Select(m => m.SmId)
                              .ToList();

            Debug.Info($"角色权限菜单ID获取成功，角色ID：{roleId}，数量：{menuIds.Count}");
            return menuIds;
        }
        catch (Exception ex)
        {
            Debug.Error($"获取角色权限菜单ID异常：{ex.Message}");
            throw;
        }
    }
}
