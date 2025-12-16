namespace WinformTemplate.Common.WebApi;

/// <summary>
/// WebAPI 客户端接口
/// </summary>
public interface IWebApiClient
{
    /// <summary>
    /// GET 请求
    /// </summary>
    Task<ApiResponse<T>> GetAsync<T>(string url, Dictionary<string, string>? headers = null);

    /// <summary>
    /// POST 请求
    /// </summary>
    Task<ApiResponse<T>> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null);

    /// <summary>
    /// PUT 请求
    /// </summary>
    Task<ApiResponse<T>> PutAsync<T>(string url, object data, Dictionary<string, string>? headers = null);

    /// <summary>
    /// DELETE 请求
    /// </summary>
    Task<ApiResponse<T>> DeleteAsync<T>(string url, Dictionary<string, string>? headers = null);

    /// <summary>
    /// 设置基础 URL
    /// </summary>
    void SetBaseUrl(string baseUrl);

    /// <summary>
    /// 设置默认超时时间（秒）
    /// </summary>
    void SetTimeout(int seconds);

    /// <summary>
    /// 设置默认请求头
    /// </summary>
    void SetDefaultHeaders(Dictionary<string, string> headers);
}
