using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.Repositories;
using Debug = WinformTemplate.Logger.Debug;

namespace WinformTemplate.Business.Sys.Service;

/// <summary>
/// 系统参数服务实现
/// </summary>
public class SysParamService : ISysParamService
{
    private readonly ISysParamRepository _paramRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    public SysParamService(ISysParamRepository paramRepository)
    {
        _paramRepository = paramRepository;
    }

    /// <summary>
    /// 获取所有参数
    /// </summary>
    public async Task<IEnumerable<SysParamModel>> GetAllParamsAsync()
    {
        try
        {
            Debug.Info("获取所有系统参数");
            var params_list = await _paramRepository.GetAllAsync();
            return params_list;
        }
        catch (Exception ex)
        {
            Debug.Error($"获取所有系统参数异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 根据ID获取参数
    /// </summary>
    public async Task<SysParamModel?> GetParamByIdAsync(long id)
    {
        try
        {
            Debug.Info($"获取系统参数，ID：{id}");
            return await _paramRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            Debug.Error($"获取系统参数异常，ID：{id}，错误：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 根据键获取参数值
    /// </summary>
    public async Task<string?> GetValueByKeyAsync(string key)
    {
        try
        {
            Debug.Info($"根据键获取参数值，Key：{key}");
            var param = await _paramRepository.GetValueByKey(key);
            return param?.SpParamValue;
        }
        catch (Exception ex)
        {
            Debug.Error($"根据键获取参数值异常，Key：{key}，错误：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 根据键设置参数值
    /// </summary>
    public async Task<bool> SetValueByKeyAsync(string key, string value)
    {
        try
        {
            Debug.Info($"设置参数值，Key：{key}，Value：{value}");

            var param = await _paramRepository.GetValueByKey(key);
            if (param == null)
            {
                // 如果参数不存在，创建新参数
                var newParam = new SysParamModel
                {
                    SpParamKey = key,
                    SpParamValue = value,
                    SpStatus = false,
                    SrCreateAt = DateTime.Now,
                    SrUpdateAt = DateTime.Now
                };
                await _paramRepository.AddAsync(newParam);
                Debug.Info($"新建参数成功，Key：{key}");
            }
            else
            {
                // 如果参数存在，更新值
                await _paramRepository.SetValueByKey(key, value);
                Debug.Info($"更新参数成功，Key：{key}");
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"设置参数值异常，Key：{key}，错误：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 创建参数
    /// </summary>
    public async Task<bool> CreateParamAsync(SysParamModel param)
    {
        try
        {
            Debug.Info($"创建系统参数：{param.SpParamKey}");

            // 检查键是否已存在
            var existingParam = await _paramRepository.GetValueByKey(param.SpParamKey);
            if (existingParam != null)
            {
                Debug.Warn($"创建系统参数失败：参数键已存在 - {param.SpParamKey}");
                return false;
            }

            // 设置创建时间
            param.SrCreateAt = DateTime.Now;
            param.SrUpdateAt = DateTime.Now;

            // 默认状态为有效（false=有效）
            if (param.SpStatus == null)
            {
                param.SpStatus = false;
            }

            await _paramRepository.AddAsync(param);
            Debug.Info($"系统参数创建成功：{param.SpParamKey}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"创建系统参数异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 更新参数
    /// </summary>
    public async Task<bool> UpdateParamAsync(SysParamModel param)
    {
        try
        {
            Debug.Info($"更新系统参数，ID：{param.SpId}");

            var existingParam = await _paramRepository.GetByIdAsync(param.SpId);
            if (existingParam == null)
            {
                Debug.Warn($"更新系统参数失败：参数不存在，ID：{param.SpId}");
                return false;
            }

            // 更新时间
            param.SrUpdateAt = DateTime.Now;

            await _paramRepository.UpdateAsync(param);
            Debug.Info($"系统参数更新成功，ID：{param.SpId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"更新系统参数异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 删除参数
    /// </summary>
    public async Task<bool> DeleteParamAsync(long id)
    {
        try
        {
            Debug.Info($"删除系统参数，ID：{id}");

            var param = await _paramRepository.GetByIdAsync(id);
            if (param == null)
            {
                Debug.Warn($"删除系统参数失败：参数不存在，ID：{id}");
                return false;
            }

            await _paramRepository.DeleteAsync(id);
            Debug.Info($"系统参数删除成功，ID：{id}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"删除系统参数异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 批量设置参数
    /// </summary>
    public async Task<bool> BatchSetParamsAsync(Dictionary<string, string> parameters)
    {
        try
        {
            Debug.Info($"批量设置参数，参数数量：{parameters.Count}");

            foreach (var kvp in parameters)
            {
                await SetValueByKeyAsync(kvp.Key, kvp.Value);
            }

            Debug.Info($"批量设置参数成功，参数数量：{parameters.Count}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Error($"批量设置参数异常：{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 检查参数键是否存在
    /// </summary>
    public async Task<bool> KeyExistsAsync(string key)
    {
        try
        {
            Debug.Info($"检查参数键是否存在，Key：{key}");
            var param = await _paramRepository.GetValueByKey(key);
            var exists = param != null;
            Debug.Info($"参数键存在检查结果：{exists}，Key：{key}");
            return exists;
        }
        catch (Exception ex)
        {
            Debug.Error($"检查参数键是否存在异常，Key：{key}，错误：{ex.Message}");
            throw;
        }
    }
}
