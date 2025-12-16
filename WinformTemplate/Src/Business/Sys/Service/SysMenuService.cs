using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.Repositories;
using Debug = WinformTemplate.Logger.Debug;

namespace WinformTemplate.Business.Sys.Service;

/// <summary>
/// 菜单服务实现
/// </summary>
public class SysMenuService : ISysMenuService
{
    private readonly ISysMenuRepository _menuRepository;
    private readonly ISysRoleRepository _roleRepository;
    private readonly ISysAccountRepository _accountRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    public SysMenuService(
        ISysMenuRepository menuRepository,
        ISysRoleRepository roleRepository,
        ISysAccountRepository accountRepository)
    {
        _menuRepository = menuRepository;
        _roleRepository = roleRepository;
        _accountRepository = accountRepository;
    }

    /// <summary>
    /// 获取所有菜单
    /// </summary>
    public async Task<IEnumerable<SysMenuModel>> GetAllMenusAsync()
    {
        try
        {
            Debug.Info("获取所有菜单");
            var menus = await _menuRepository.GetAllAsync();
            return menus;
        }
        catch (Exception ex)
        {
            Debug.Error($"获取所有菜单异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 获取菜单树（树形结构）
    /// </summary>
    public async Task<IEnumerable<SysMenuModel>> GetMenuTreeAsync()
    {
        try
        {
            Debug.Info("获取菜单树");
            var allMenus = await _menuRepository.GetAllAsync();

            // 过滤有效菜单（SysStatus=false表示有效）
            var validMenus = allMenus.Where(m => m.SysStatus == false).ToList();

            // 按排序规则排序
            validMenus = validMenus.OrderBy(m => m.SmSort ?? int.MaxValue).ToList();

            Debug.Info($"菜单树获取成功，有效菜单数量：{validMenus.Count}");
            return validMenus;
        }
        catch (Exception ex)
        {
            Debug.Error($"获取菜单树异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 根据角色获取菜单
    /// </summary>
    public async Task<IEnumerable<SysMenuModel>> GetMenusByRoleIdAsync(long roleId)
    {
        try
        {
            Debug.Info($"根据角色获取菜单，角色ID：{roleId}");

            var menus = await _roleRepository.GetMenusByRoleIdAsync(roleId);

            // 过滤有效菜单
            var validMenus = menus.Where(m => m.SysStatus == false)
                                  .OrderBy(m => m.SmSort ?? int.MaxValue)
                                  .ToList();

            Debug.Info($"角色菜单获取成功，角色ID：{roleId}，菜单数量：{validMenus.Count}");
            return validMenus;
        }
        catch (Exception ex)
        {
            Debug.Error($"根据角色获取菜单异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 根据账户获取菜单（考虑角色权限）
    /// </summary>
    public async Task<IEnumerable<SysMenuModel>> GetMenusByAccountIdAsync(long accountId)
    {
        try
        {
            Debug.Info($"根据账户获取菜单，账户ID：{accountId}");

            // 获取账户信息
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null || account.SysStatus == true)
            {
                Debug.Warn($"获取账户菜单失败：账户不存在或已冻结，ID：{accountId}");
                return Enumerable.Empty<SysMenuModel>();
            }

            // 如果没有角色，返回空列表
            if (account.SysRoleId == null)
            {
                Debug.Warn($"获取账户菜单：账户没有分配角色，ID：{accountId}");
                return Enumerable.Empty<SysMenuModel>();
            }

            // 获取角色的所有菜单权限
            var menus = await GetMenusByRoleIdAsync(account.SysRoleId.Value);

            Debug.Info($"账户菜单获取成功，账户ID：{accountId}，菜单数量：{menus.Count()}");
            return menus;
        }
        catch (Exception ex)
        {
            Debug.Error($"根据账户获取菜单异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 根据ID获取菜单
    /// </summary>
    public async Task<SysMenuModel?> GetMenuByIdAsync(long id)
    {
        try
        {
            Debug.Info($"获取菜单，ID：{id}");
            return await _menuRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            Debug.Error($"获取菜单异常，ID：{id}，错误：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 创建菜单
    /// </summary>
    public async Task<bool> CreateMenuAsync(SysMenuModel menu)
    {
        try
        {
            Debug.Info($"创建菜单：{menu.SmName}");

            // 设置创建时间
            menu.SysCreateAt = DateTime.Now;
            menu.SysUpdateAt = DateTime.Now;

            // 默认状态为有效（false=有效）
            if (menu.SysStatus == null)
            {
                menu.SysStatus = false;
            }

            await _menuRepository.AddAsync(menu);
            Debug.Info($"菜单创建成功：{menu.SmName}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"创建菜单异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 更新菜单
    /// </summary>
    public async Task<bool> UpdateMenuAsync(SysMenuModel menu)
    {
        try
        {
            Debug.Info($"更新菜单，ID：{menu.SmId}");

            var existingMenu = await _menuRepository.GetByIdAsync(menu.SmId);
            if (existingMenu == null)
            {
                Debug.Warn($"更新菜单失败：菜单不存在，ID：{menu.SmId}");
                return false;
            }

            // 更新时间
            menu.SysUpdateAt = DateTime.Now;

            await _menuRepository.UpdateAsync(menu);
            Debug.Info($"菜单更新成功，ID：{menu.SmId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"更新菜单异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 删除菜单
    /// </summary>
    public async Task<bool> DeleteMenuAsync(long id)
    {
        try
        {
            Debug.Info($"删除菜单，ID：{id}");

            var menu = await _menuRepository.GetByIdAsync(id);
            if (menu == null)
            {
                Debug.Warn($"删除菜单失败：菜单不存在，ID：{id}");
                return false;
            }

            // 检查是否有子菜单
            var childMenus = await GetChildMenusAsync(id);
            if (childMenus.Any())
            {
                Debug.Warn($"删除菜单失败：存在子菜单，ID：{id}");
                return false;
            }

            await _menuRepository.DeleteAsync(id);
            Debug.Info($"菜单删除成功，ID：{id}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"删除菜单异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 冻结菜单
    /// </summary>
    public async Task<bool> FreezeMenuAsync(long id)
    {
        try
        {
            Debug.Info($"冻结菜单，ID：{id}");

            var menu = await _menuRepository.GetByIdAsync(id);
            if (menu == null)
            {
                Debug.Warn($"冻结菜单失败：菜单不存在，ID：{id}");
                return false;
            }

            await _menuRepository.FreezeMenuAsync(id);
            Debug.Info($"菜单冻结成功，ID：{id}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"冻结菜单异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 解冻菜单
    /// </summary>
    public async Task<bool> UnfreezeMenuAsync(long id)
    {
        try
        {
            Debug.Info($"解冻菜单，ID：{id}");

            var menu = await _menuRepository.GetByIdAsync(id);
            if (menu == null)
            {
                Debug.Warn($"解冻菜单失败：菜单不存在，ID：{id}");
                return false;
            }

            await _menuRepository.UnfreezeMenuAsync(id);
            Debug.Info($"菜单解冻成功，ID：{id}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"解冻菜单异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 获取子菜单
    /// </summary>
    public async Task<IEnumerable<SysMenuModel>> GetChildMenusAsync(long parentId)
    {
        try
        {
            Debug.Info($"获取子菜单，父菜单ID：{parentId}");

            var allMenus = await _menuRepository.GetAllAsync();
            var childMenus = allMenus.Where(m => m.SmParentId == parentId)
                                    .OrderBy(m => m.SmSort ?? int.MaxValue)
                                    .ToList();

            Debug.Info($"子菜单获取成功，父菜单ID：{parentId}，子菜单数量：{childMenus.Count}");
            return childMenus;
        }
        catch (Exception ex)
        {
            Debug.Error($"获取子菜单异常：{ex.Message}");
            throw;
        }
    }
}
