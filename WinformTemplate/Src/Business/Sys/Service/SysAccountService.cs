using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.Repositories;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Tools.Encryption;
using Debug = WinformTemplate.Logger.Debug;

namespace WinformTemplate.Business.Sys.Service;

/// <summary>
/// 账户服务实现
/// </summary>
public class SysAccountService : ISysAccountService
{
    private const int DefaultPageSize = 20;

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

            var account = await _accountRepository.GetByUsernameAsync(username);

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

            var isValidPassword = PasswordHasher.VerifyPassword(password, account.SysPassword, out var needsRehash);
            if (!isValidPassword)
            {
                Debug.Warn($"登录失败：密码错误 - {username}");
                return null;
            }

            if (needsRehash && WinformTemplate.Serialize.GlobalProjectConfig.Instance.Config?.Security.UpgradeLegacyPasswordHashOnLogin == true)
            {
                account.SysPassword = PasswordHasher.HashPassword(password);
                await _accountRepository.UpdateAsync(account);
                Debug.Info($"已迁移旧密码哈希 - 用户：{username}");
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
            var result = await QueryAccountsAsync(pageSize: DefaultPageSize);
            return result.Items;
        }
        catch (Exception ex)
        {
            Debug.Error($"获取所有账户异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 搜索账户
    /// </summary>
    public async Task<IEnumerable<SysAccountModel>> SearchAccountsAsync(string keyword)
    {
        try
        {
            Debug.Info($"搜索账户：{keyword}");
            var result = await QueryAccountsAsync(keyword, pageSize: DefaultPageSize);
            return result.Items;
        }
        catch (Exception ex)
        {
            Debug.Error($"搜索账户异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 根据ID获取账户
    /// </summary>
    public async Task<PagedResult<SysAccountModel>> QueryAccountsAsync(string? keyword = null, int page = 1, int pageSize = DefaultPageSize)
    {
        return await _accountRepository.QueryAsync(new QueryRequest
        {
            Page = Math.Max(page, 1),
            PageSize = Math.Max(pageSize, 1),
            Keyword = keyword,
            SortBy = nameof(SysAccountModel.SysId)
        });
    }

    public async Task<SysAccountModel?> GetAccountByIdAsync(long id)
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
            return await _accountRepository.GetByUsernameAsync(username);
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

            if (!string.IsNullOrEmpty(account.SysPassword))
            {
                account.SysPassword = PasswordHasher.HashPassword(account.SysPassword);
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

            if (!string.IsNullOrWhiteSpace(account.SysPassword))
            {
                account.SysPassword = PasswordHasher.HashPassword(account.SysPassword);
            }
            else
            {
                account.SysPassword = existingAccount.SysPassword;
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
    public async Task<bool> DeleteAccountAsync(long id)
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
    public async Task<bool> ChangePasswordAsync(long accountId, string oldPassword, string newPassword)
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

            if (!PasswordHasher.VerifyPassword(oldPassword, account.SysPassword, out _))
            {
                Debug.Warn($"修改密码失败：旧密码错误，ID：{accountId}");
                return false;
            }

            // 设置新密码
            account.SysPassword = PasswordHasher.HashPassword(newPassword);
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
    public async Task<bool> FreezeAccountAsync(long accountId)
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
    public async Task<bool> UnfreezeAccountAsync(long accountId)
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
    public async Task<bool> HasPermissionAsync(long accountId, long menuId)
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
    public async Task<IEnumerable<SysMenuModel>> GetAccountMenusAsync(long accountId)
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

            var menuList = menus.ToList();
            Debug.Info($"获取账户菜单成功，账户ID：{accountId}，菜单数量：{menuList.Count}");
            return menuList;
        }
        catch (Exception ex)
        {
            Debug.Error($"获取账户菜单异常：{ex.Message}");
            throw;
        }
    }
}
