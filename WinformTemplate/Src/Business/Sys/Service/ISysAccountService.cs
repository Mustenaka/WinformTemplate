using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

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
    /// 搜索账户
    /// </summary>
    Task<IEnumerable<SysAccountModel>> SearchAccountsAsync(string keyword);

    Task<PagedResult<SysAccountModel>> QueryAccountsAsync(string? keyword = null, int page = 1, int pageSize = 20);

    /// <summary>
    /// 根据ID获取账户
    /// </summary>
    Task<SysAccountModel?> GetAccountByIdAsync(long id);

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
    Task<bool> DeleteAccountAsync(long id);

    /// <summary>
    /// 修改密码
    /// </summary>
    Task<bool> ChangePasswordAsync(long accountId, string oldPassword, string newPassword);

    /// <summary>
    /// 冻结账户
    /// </summary>
    Task<bool> FreezeAccountAsync(long accountId);

    /// <summary>
    /// 解冻账户
    /// </summary>
    Task<bool> UnfreezeAccountAsync(long accountId);

    /// <summary>
    /// 验证账户权限
    /// </summary>
    Task<bool> HasPermissionAsync(long accountId, long menuId);

    /// <summary>
    /// 获取账户的所有权限菜单
    /// </summary>
    Task<IEnumerable<SysMenuModel>> GetAccountMenusAsync(long accountId);
}
