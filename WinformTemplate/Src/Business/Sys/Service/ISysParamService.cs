using WinformTemplate.Business.Sys.Model;

namespace WinformTemplate.Business.Sys.Service;

/// <summary>
/// 系统参数服务接口
/// </summary>
public interface ISysParamService
{
    /// <summary>
    /// 获取所有参数
    /// </summary>
    Task<IEnumerable<SysParamModel>> GetAllParamsAsync();

    /// <summary>
    /// 根据ID获取参数
    /// </summary>
    Task<SysParamModel?> GetParamByIdAsync(long id);

    /// <summary>
    /// 根据键获取参数值
    /// </summary>
    Task<string?> GetValueByKeyAsync(string key);

    /// <summary>
    /// 根据键设置参数值
    /// </summary>
    Task<bool> SetValueByKeyAsync(string key, string value);

    /// <summary>
    /// 创建参数
    /// </summary>
    Task<bool> CreateParamAsync(SysParamModel param);

    /// <summary>
    /// 更新参数
    /// </summary>
    Task<bool> UpdateParamAsync(SysParamModel param);

    /// <summary>
    /// 删除参数
    /// </summary>
    Task<bool> DeleteParamAsync(long id);

    /// <summary>
    /// 批量设置参数
    /// </summary>
    Task<bool> BatchSetParamsAsync(Dictionary<string, string> parameters);

    /// <summary>
    /// 检查参数键是否存在
    /// </summary>
    Task<bool> KeyExistsAsync(string key);
}
