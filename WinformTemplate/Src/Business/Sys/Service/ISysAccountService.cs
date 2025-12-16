using WinformTemplate.Business.Sys.Model;

namespace WinformTemplate.Business.Sys.Service;

/// <summary>
/// 账户服务接口
/// </summary>
public interface ISysAccountService
{
    /// <summary>
    /// 用户登录
    /// </summary>
    Task<SysAccountModel?> LoginAsync(string username, string password);

    /// <summary>
    /// 获取所有账户
    /// </summary>
    Task<IEnumerable<SysAccountModel>> GetAllAccountsAsync();

    /// <summary>
    /// 根据ID获取账户
    /// </summary>
    Task<SysAccountModel?> GetAccountByIdAsync(int id);

    /// <summary>
    /// 根据用户名获取账户
    /// </summary>
    Task<SysAccountModel?> GetAccountByUsernameAsync(string username);

    /// <summary>
    /// 创建账户
    /// </summary>
    Task<bool> CreateAccountAsync(SysAccountModel account);

    /// <summary>
    /// 更新账户
    /// </summary>
    Task<bool> UpdateAccountAsync(SysAccountModel account);

    /// <summary>
    /// 删除账户
    /// </summary>
    Task<bool> DeleteAccountAsync(int id);

    /// <summary>
    /// 修改密码
    /// </summary>
    Task<bool> ChangePasswordAsync(int accountId, string oldPassword, string newPassword);

    /// <summary>
    /// 冻结账户
    /// </summary>
    Task<bool> FreezeAccountAsync(int accountId);

    /// <summary>
    /// 解冻账户
    /// </summary>
    Task<bool> UnfreezeAccountAsync(int accountId);

    /// <summary>
    /// 验证账户权限
    /// </summary>
    Task<bool> HasPermissionAsync(int accountId, int menuId);

    /// <summary>
    /// 获取账户的所有权限菜单
    /// </summary>
    Task<IEnumerable<SysMenuModel>> GetAccountMenusAsync(int accountId);
}
