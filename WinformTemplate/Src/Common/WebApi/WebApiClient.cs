using Newtonsoft.Json;
using System.Text;
using WinformTemplate.Logger;

namespace WinformTemplate.Common.WebApi;

/// <summary>
/// WebAPI 客户端实现
/// </summary>
public class WebApiClient : IWebApiClient
{
    private readonly HttpClient _httpClient;
    private string _baseUrl = string.Empty;
    private readonly Dictionary<string, string> _defaultHeaders = new();

    public WebApiClient()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// 设置基础 URL
    /// </summary>
    public void SetBaseUrl(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        Debug.Info($"WebAPI BaseUrl 设置为: {_baseUrl}");
    }

    /// <summary>
    /// 设置超时时间
    /// </summary>
    public void SetTimeout(int seconds)
    {
        _httpClient.Timeout = TimeSpan.FromSeconds(seconds);
        Debug.Info($"WebAPI Timeout 设置为: {seconds}秒");
    }

    /// <summary>
    /// 设置默认请求头
    /// </summary>
    public void SetDefaultHeaders(Dictionary<string, string> headers)
    {
        _defaultHeaders.Clear();
        foreach (var header in headers)
        {
            _defaultHeaders[header.Key] = header.Value;
        }
        Debug.Info($"WebAPI 默认请求头已设置，共 {headers.Count} 个");
    }

    /// <summary>
    /// GET 请求
    /// </summary>
    public async Task<ApiResponse<T>> GetAsync<T>(string url, Dictionary<string, string>? headers = null)
    {
        try
        {
            var fullUrl = BuildFullUrl(url);
            Debug.Info($"WebAPI GET: {fullUrl}");

            using var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            AddHeaders(request, headers);

            var response = await _httpClient.SendAsync(request);
            return await ProcessResponse<T>(response);
        }
        catch (Exception ex)
        {
            Debug.Error($"WebAPI GET 请求失败: {url}", ex);
            return ApiResponse<T>.CreateError($"请求失败: {ex.Message}");
        }
    }

    /// <summary>
    /// POST 请求
    /// </summary>
    public async Task<ApiResponse<T>> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null)
    {
        try
        {
            var fullUrl = BuildFullUrl(url);
            Debug.Info($"WebAPI POST: {fullUrl}");

            using var request = new HttpRequestMessage(HttpMethod.Post, fullUrl);
            AddHeaders(request, headers);

            var json = JsonConvert.SerializeObject(data);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            return await ProcessResponse<T>(response);
        }
        catch (Exception ex)
        {
            Debug.Error($"WebAPI POST 请求失败: {url}", ex);
            return ApiResponse<T>.CreateError($"请求失败: {ex.Message}");
        }
    }

    /// <summary>
    /// PUT 请求
    /// </summary>
    public async Task<ApiResponse<T>> PutAsync<T>(string url, object data, Dictionary<string, string>? headers = null)
    {
        try
        {
            var fullUrl = BuildFullUrl(url);
            Debug.Info($"WebAPI PUT: {fullUrl}");

            using var request = new HttpRequestMessage(HttpMethod.Put, fullUrl);
            AddHeaders(request, headers);

            var json = JsonConvert.SerializeObject(data);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            return await ProcessResponse<T>(response);
        }
        catch (Exception ex)
        {
            Debug.Error($"WebAPI PUT 请求失败: {url}", ex);
            return ApiResponse<T>.CreateError($"请求失败: {ex.Message}");
        }
    }

    /// <summary>
    /// DELETE 请求
    /// </summary>
    public async Task<ApiResponse<T>> DeleteAsync<T>(string url, Dictionary<string, string>? headers = null)
    {
        try
        {
            var fullUrl = BuildFullUrl(url);
            Debug.Info($"WebAPI DELETE: {fullUrl}");

            using var request = new HttpRequestMessage(HttpMethod.Delete, fullUrl);
            AddHeaders(request, headers);

            var response = await _httpClient.SendAsync(request);
            return await ProcessResponse<T>(response);
        }
        catch (Exception ex)
        {
            Debug.Error($"WebAPI DELETE 请求失败: {url}", ex);
            return ApiResponse<T>.CreateError($"请求失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 构建完整 URL
    /// </summary>
    private string BuildFullUrl(string url)
    {
        if (url.StartsWith("http://") || url.StartsWith("https://"))
        {
            return url;
        }

        if (string.IsNullOrEmpty(_baseUrl))
        {
            throw new InvalidOperationException("BaseUrl 未设置，请先调用 SetBaseUrl 方法");
        }

        return $"{_baseUrl}/{url.TrimStart('/')}";
    }

    /// <summary>
    /// 添加请求头
    /// </summary>
    private void AddHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
    {
        // 添加默认请求头
        foreach (var header in _defaultHeaders)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // 添加自定义请求头
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }

    /// <summary>
    /// 处理响应
    /// </summary>
    private async Task<ApiResponse<T>> ProcessResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Debug.Warn($"WebAPI 响应失败: {response.StatusCode}, 内容: {content}");
            return ApiResponse<T>.CreateError(
                $"请求失败: {response.StatusCode}",
                (int)response.StatusCode
            );
        }

        try
        {
            // 尝试直接反序列化为 ApiResponse<T>
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(content);
            if (apiResponse != null)
            {
                return apiResponse;
            }

            // 如果不是 ApiResponse 格式，尝试直接反序列化为 T
            var data = JsonConvert.DeserializeObject<T>(content);
            return ApiResponse<T>.CreateSuccess(data);
        }
        catch (Exception ex)
        {
            Debug.Error("WebAPI 响应解析失败", ex);
            return ApiResponse<T>.CreateError($"响应解析失败: {ex.Message}");
        }
    }
}
