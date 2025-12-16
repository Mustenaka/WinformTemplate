using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.Repositories;
using WinformTemplate.Tools.Encryption;
using Debug = WinformTemplate.Logger.Debug;

namespace WinformTemplate.Business.Sys.Service;

/// <summary>
/// 账户服务实现
/// </summary>
public class SysAccountService : ISysAccountService
{
    private readonly ISysAccountRepository _accountRepository;
    private readonly ISysRoleRepository _roleRepository;
    private readonly ISysMenuRepository _menuRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    public SysAccountService(
        ISysAccountRepository accountRepository,
        ISysRoleRepository roleRepository,
        ISysMenuRepository menuRepository)
    {
        _accountRepository = accountRepository;
        _roleRepository = roleRepository;
        _menuRepository = menuRepository;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码（明文）</param>
    /// <returns>登录成功返回账户信息，失败返回null</returns>
    public async Task<SysAccountModel?> LoginAsync(string username, string password)
    {
        try
        {
            Debug.Info($"尝试登录，用户名：{username}");

            // 获取所有账户并查找匹配的用户
            var accounts = await _accountRepository.GetAllAsync();
            var account = accounts.FirstOrDefault(a => a.SysAccountName == username);

            if (account == null)
            {
                Debug.Warn($"登录失败：用户名不存在 - {username}");
                return null;
            }

            // 检查账户状态（false=有效，true=无效）
            if (account.SysStatus == true)
            {
                Debug.Warn($"登录失败：账户已冻结 - {username}");
                return null;
            }

            // 验证密码（使用MD5加密后比较）
            var passwordHash = MD5Helper.Encrypt(password);
            if (account.SysPassword != passwordHash)
            {
                Debug.Warn($"登录失败：密码错误 - {username}");
                return null;
            }

            Debug.Info($"登录成功 - 用户：{username}，角色ID：{account.SysRoleId}");
            return account;
        }
        catch (Exception ex)
        {
            Debug.Error($"登录异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 获取所有账户
    /// </summary>
    public async Task<IEnumerable<SysAccountModel>> GetAllAccountsAsync()
    {
        try
        {
            Debug.Info("获取所有账户");
            var accounts = await _accountRepository.GetAllAsync();
            return accounts;
        }
        catch (Exception ex)
        {
            Debug.Error($"获取所有账户异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 根据ID获取账户
    /// </summary>
    public async Task<SysAccountModel?> GetAccountByIdAsync(int id)
    {
        try
        {
            Debug.Info($"获取账户，ID：{id}");
            return await _accountRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            Debug.Error($"获取账户异常，ID：{id}，错误：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 根据用户名获取账户
    /// </summary>
    public async Task<SysAccountModel?> GetAccountByUsernameAsync(string username)
    {
        try
        {
            Debug.Info($"根据用户名获取账户：{username}");
            var accounts = await _accountRepository.GetAllAsync();
            return accounts.FirstOrDefault(a => a.SysAccountName == username);
        }
        catch (Exception ex)
        {
            Debug.Error($"根据用户名获取账户异常：{username}，错误：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 创建账户
    /// </summary>
    public async Task<bool> CreateAccountAsync(SysAccountModel account)
    {
        try
        {
            Debug.Info($"创建账户：{account.SysAccountName}");

            // 检查用户名是否已存在
            var existingAccount = await GetAccountByUsernameAsync(account.SysAccountName!);
            if (existingAccount != null)
            {
                Debug.Warn($"创建账户失败：用户名已存在 - {account.SysAccountName}");
                return false;
            }

            // 密码使用MD5加密
            if (!string.IsNullOrEmpty(account.SysPassword))
            {
                account.SysPassword = MD5Helper.Encrypt(account.SysPassword);
            }

            // 设置创建时间
            account.SysCreateAt = DateTime.Now;
            account.SysUpdateAt = DateTime.Now;

            // 默认状态为有效（false=有效）
            if (account.SysStatus == null)
            {
                account.SysStatus = false;
            }

            await _accountRepository.AddAsync(account);
            Debug.Info($"账户创建成功：{account.SysAccountName}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"创建账户异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 更新账户
    /// </summary>
    public async Task<bool> UpdateAccountAsync(SysAccountModel account)
    {
        try
        {
            Debug.Info($"更新账户，ID：{account.SysId}");

            var existingAccount = await _accountRepository.GetByIdAsync(account.SysId);
            if (existingAccount == null)
            {
                Debug.Warn($"更新账户失败：账户不存在，ID：{account.SysId}");
                return false;
            }

            // 更新时间
            account.SysUpdateAt = DateTime.Now;

            await _accountRepository.UpdateAsync(account);
            Debug.Info($"账户更新成功，ID：{account.SysId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"更新账户异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 删除账户
    /// </summary>
    public async Task<bool> DeleteAccountAsync(int id)
    {
        try
        {
            Debug.Info($"删除账户，ID：{id}");

            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null)
            {
                Debug.Warn($"删除账户失败：账户不存在，ID：{id}");
                return false;
            }

            await _accountRepository.DeleteAsync(id);
            Debug.Info($"账户删除成功，ID：{id}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"删除账户异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <param name="oldPassword">旧密码（明文）</param>
    /// <param name="newPassword">新密码（明文）</param>
    public async Task<bool> ChangePasswordAsync(int accountId, string oldPassword, string newPassword)
    {
        try
        {
            Debug.Info($"修改密码，账户ID：{accountId}");

            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                Debug.Warn($"修改密码失败：账户不存在，ID：{accountId}");
                return false;
            }

            // 验证旧密码
            var oldPasswordHash = MD5Helper.Encrypt(oldPassword);
            if (account.SysPassword != oldPasswordHash)
            {
                Debug.Warn($"修改密码失败：旧密码错误，ID：{accountId}");
                return false;
            }

            // 设置新密码
            account.SysPassword = MD5Helper.Encrypt(newPassword);
            account.SysUpdateAt = DateTime.Now;

            await _accountRepository.UpdateAsync(account);
            Debug.Info($"密码修改成功，账户ID：{accountId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"修改密码异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 冻结账户
    /// </summary>
    public async Task<bool> FreezeAccountAsync(int accountId)
    {
        try
        {
            Debug.Info($"冻结账户，ID：{accountId}");

            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                Debug.Warn($"冻结账户失败：账户不存在，ID：{accountId}");
                return false;
            }

            await _accountRepository.FreezeAccountAsync(accountId);
            Debug.Info($"账户冻结成功，ID：{accountId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"冻结账户异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 解冻账户
    /// </summary>
    public async Task<bool> UnfreezeAccountAsync(int accountId)
    {
        try
        {
            Debug.Info($"解冻账户，ID：{accountId}");

            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                Debug.Warn($"解冻账户失败：账户不存在，ID：{accountId}");
                return false;
            }

            await _accountRepository.UnfreezeAccountAsync(accountId);
            Debug.Info($"账户解冻成功，ID：{accountId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"解冻账户异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 验证账户权限
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <param name="menuId">菜单ID</param>
    /// <returns>是否有权限</returns>
    public async Task<bool> HasPermissionAsync(int accountId, int menuId)
    {
        try
        {
            Debug.Info($"验证权限，账户ID：{accountId}，菜单ID：{menuId}");

            // 获取账户信息
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null || account.SysStatus == true)
            {
                Debug.Warn($"验证权限失败：账户不存在或已冻结，ID：{accountId}");
                return false;
            }

            // 如果没有角色，无权限
            if (account.SysRoleId == null)
            {
                Debug.Warn($"验证权限失败：账户没有分配角色，ID：{accountId}");
                return false;
            }

            // 获取角色的所有菜单权限
            var roleMenus = await _roleRepository.GetMenusByRoleIdAsync(account.SysRoleId.Value);
            var hasPermission = roleMenus.Any(m => m.SmId == menuId);

            Debug.Info($"权限验证结果：{hasPermission}，账户ID：{accountId}，菜单ID：{menuId}");
            return hasPermission;
        }
        catch (Exception ex)
        {
            Debug.Error($"验证权限异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 获取账户的所有权限菜单
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <returns>菜单列表</returns>
    public async Task<IEnumerable<SysMenuModel>> GetAccountMenusAsync(int accountId)
    {
        try
        {
            Debug.Info($"获取账户菜单，账户ID：{accountId}");

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
            var menus = await _roleRepository.GetMenusByRoleIdAsync(account.SysRoleId.Value);

            // 过滤掉无效的菜单（SysStatus=true表示无效）
            var validMenus = menus.Where(m => m.SysStatus == false).ToList();

            Debug.Info($"获取账户菜单成功，账户ID：{accountId}，菜单数量：{validMenus.Count}");
            return validMenus;
        }
        catch (Exception ex)
        {
            Debug.Error($"获取账户菜单异常：{ex.Message}");
            throw;
        }
    }
}
