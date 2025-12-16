using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.Repositories;
using Debug = WinformTemplate.Logger.Debug;

namespace WinformTemplate.Business.Sys.Service;

/// <summary>
/// 角色服务实现
/// </summary>
public class SysRoleService : ISysRoleService
{
    private readonly ISysRoleRepository _roleRepository;
    private readonly ISysMenuRepository _menuRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    public SysRoleService(
        ISysRoleRepository roleRepository,
        ISysMenuRepository menuRepository)
    {
        _roleRepository = roleRepository;
        _menuRepository = menuRepository;
    }

    /// <summary>
    /// 获取所有角色
    /// </summary>
    public async Task<IEnumerable<SysRoleModel>> GetAllRolesAsync()
    {
        try
        {
            Debug.Info("获取所有角色");
            var roles = await _roleRepository.GetAllAsync();
            return roles;
        }
        catch (Exception ex)
        {
            Debug.Error($"获取所有角色异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 根据ID获取角色
    /// </summary>
    public async Task<SysRoleModel?> GetRoleByIdAsync(long id)
    {
        try
        {
            Debug.Info($"获取角色，ID：{id}");
            return await _roleRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            Debug.Error($"获取角色异常，ID：{id}，错误：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    public async Task<bool> CreateRoleAsync(SysRoleModel role)
    {
        try
        {
            Debug.Info($"创建角色：{role.SrName}");

            // 设置创建时间
            role.SrCreateAt = DateTime.Now;
            role.SrUpdateAt = DateTime.Now;

            // 默认状态为有效（false=有效）
            if (role.SrStatus == null)
            {
                role.SrStatus = false;
            }

            await _roleRepository.AddAsync(role);
            Debug.Info($"角色创建成功：{role.SrName}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"创建角色异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    public async Task<bool> UpdateRoleAsync(SysRoleModel role)
    {
        try
        {
            Debug.Info($"更新角色，ID：{role.SrId}");

            var existingRole = await _roleRepository.GetByIdAsync(role.SrId);
            if (existingRole == null)
            {
                Debug.Warn($"更新角色失败：角色不存在，ID：{role.SrId}");
                return false;
            }

            // 更新时间
            role.SrUpdateAt = DateTime.Now;

            await _roleRepository.UpdateAsync(role);
            Debug.Info($"角色更新成功，ID：{role.SrId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"更新角色异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    public async Task<bool> DeleteRoleAsync(long id)
    {
        try
        {
            Debug.Info($"删除角色，ID：{id}");

            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                Debug.Warn($"删除角色失败：角色不存在，ID：{id}");
                return false;
            }

            await _roleRepository.DeleteAsync(id);
            Debug.Info($"角色删除成功，ID：{id}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"删除角色异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 分配菜单权限给角色
    /// </summary>
    public async Task<bool> AssignMenuToRoleAsync(long roleId, long menuId)
    {
        try
        {
            Debug.Info($"分配菜单权限，角色ID：{roleId}，菜单ID：{menuId}");

            // 检查角色是否存在
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                Debug.Warn($"分配菜单权限失败：角色不存在，ID：{roleId}");
                return false;
            }

            // 检查菜单是否存在
            var menu = await _menuRepository.GetByIdAsync(menuId);
            if (menu == null)
            {
                Debug.Warn($"分配菜单权限失败：菜单不存在，ID：{menuId}");
                return false;
            }

            // 检查是否已经分配
            var hasPermission = await HasMenuPermissionAsync(roleId, menuId);
            if (hasPermission)
            {
                Debug.Warn($"分配菜单权限：权限已存在，角色ID：{roleId}，菜单ID：{menuId}");
                return true;
            }

            await _roleRepository.AssignMenuToRoleAsync(roleId, menuId);
            Debug.Info($"菜单权限分配成功，角色ID：{roleId}，菜单ID：{menuId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"分配菜单权限异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 批量分配菜单权限给角色
    /// </summary>
    public async Task<bool> AssignMenusToRoleAsync(long roleId, IEnumerable<long> menuIds)
    {
        try
        {
            Debug.Info($"批量分配菜单权限，角色ID：{roleId}，菜单数量：{menuIds.Count()}");

            // 检查角色是否存在
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                Debug.Warn($"批量分配菜单权限失败：角色不存在，ID：{roleId}");
                return false;
            }

            foreach (var menuId in menuIds)
            {
                await AssignMenuToRoleAsync(roleId, menuId);
            }

            Debug.Info($"批量分配菜单权限成功，角色ID：{roleId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"批量分配菜单权限异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 移除角色的菜单权限
    /// </summary>
    public async Task<bool> RemoveMenuFromRoleAsync(long roleId, long menuId)
    {
        try
        {
            Debug.Info($"移除菜单权限，角色ID：{roleId}，菜单ID：{menuId}");

            // 检查角色是否存在
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                Debug.Warn($"移除菜单权限失败：角色不存在，ID：{roleId}");
                return false;
            }

            await _roleRepository.RemoveMenuFromRoleAsync(roleId, menuId);
            Debug.Info($"菜单权限移除成功，角色ID：{roleId}，菜单ID：{menuId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"移除菜单权限异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 获取角色的所有权限菜单
    /// </summary>
    public async Task<IEnumerable<SysMenuModel>> GetRoleMenusAsync(long roleId)
    {
        try
        {
            Debug.Info($"获取角色菜单，角色ID：{roleId}");

            var menus = await _roleRepository.GetMenusByRoleIdAsync(roleId);

            // 过滤有效菜单并排序
            var validMenus = menus.Where(m => m.SysStatus == false)
                                  .OrderBy(m => m.SmSort ?? int.MaxValue)
                                  .ToList();

            Debug.Info($"角色菜单获取成功，角色ID：{roleId}，菜单数量：{validMenus.Count}");
            return validMenus;
        }
        catch (Exception ex)
        {
            Debug.Error($"获取角色菜单异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 检查角色是否有指定菜单权限
    /// </summary>
    public async Task<bool> HasMenuPermissionAsync(long roleId, long menuId)
    {
        try
        {
            Debug.Info($"检查角色菜单权限，角色ID：{roleId}，菜单ID：{menuId}");

            var menus = await _roleRepository.GetMenusByRoleIdAsync(roleId);
            var hasPermission = menus.Any(m => m.SmId == menuId);

            Debug.Info($"角色菜单权限检查结果：{hasPermission}，角色ID：{roleId}，菜单ID：{menuId}");
            return hasPermission;
        }
        catch (Exception ex)
        {
            Debug.Error($"检查角色菜单权限异常：{ex.Message}");
            throw;
        }
    }
}
