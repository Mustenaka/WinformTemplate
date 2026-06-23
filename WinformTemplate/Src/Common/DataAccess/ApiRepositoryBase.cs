using System.Globalization;
using System.Text;
using WinformTemplate.Logger;

namespace WinformTemplate.Common.DataAccess;

public abstract class ApiRepositoryBase<T> : IRepository<T> where T : class
{
    private readonly IWebApiClient _client;
    private readonly string _moduleKey;
    private readonly string _resource;

    protected ApiRepositoryBase(IWebApiClient client, string moduleKey, string resource)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _moduleKey = moduleKey;
        _resource = resource;
    }

    protected string CollectionEndpoint => $"/api/{_moduleKey}/{_resource}";

    public virtual async Task<T?> GetByIdAsync(object id)
    {
        var endpoint = $"{CollectionEndpoint}/{Uri.EscapeDataString(Convert.ToString(id, CultureInfo.InvariantCulture) ?? string.Empty)}";
        var response = await _client.GetAsync<T>(endpoint);
        return HandleOptionalResponse(response, endpoint);
    }

    public virtual async Task<PagedResult<T>> QueryAsync(QueryRequest req)
    {
        var endpoint = $"{CollectionEndpoint}{BuildQueryString(req)}";
        var response = await _client.GetAsync<PagedResult<T>>(endpoint);
        return HandleOptionalResponse(response, endpoint) ?? new PagedResult<T>();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        var endpoint = CollectionEndpoint;
        var response = await _client.PostAsync<T>(endpoint, entity);
        var created = HandleOptionalResponse(response, endpoint);
        if (created == null)
        {
            throw new InvalidOperationException(response.Message ?? "新增失败");
        }

        return created;
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        var endpoint = $"{CollectionEndpoint}/{Uri.EscapeDataString(Convert.ToString(GetEntityId(entity), CultureInfo.InvariantCulture) ?? string.Empty)}";
        var response = await _client.PutAsync<bool>(endpoint, entity);
        return HandleBooleanResponse(response, endpoint);
    }

    public virtual async Task<bool> DeleteAsync(object id)
    {
        var endpoint = $"{CollectionEndpoint}/{Uri.EscapeDataString(Convert.ToString(id, CultureInfo.InvariantCulture) ?? string.Empty)}";
        var response = await _client.DeleteAsync<bool>(endpoint);
        return HandleBooleanResponse(response, endpoint);
    }

    protected abstract object GetEntityId(T entity);

    protected async Task<TResponse?> GetOptionalAsync<TResponse>(string endpoint)
    {
        var response = await _client.GetAsync<TResponse>(endpoint);
        return HandleOptionalResponse(response, endpoint);
    }

    protected async Task<bool> PostBooleanAsync(string endpoint, object body)
    {
        var response = await _client.PostAsync<bool>(endpoint, body);
        return HandleBooleanResponse(response, endpoint);
    }

    protected TResponse? HandleOptionalResponse<TResponse>(ApiResponse<TResponse> response, string endpoint)
    {
        ThrowIfUnavailable(response, endpoint);
        if (response.Success)
        {
            return response.Data;
        }

        Debug.Warn($"{_moduleKey} WebAPI 业务失败: {endpoint}, {response.Message}");
        return default;
    }

    protected bool HandleBooleanResponse(ApiResponse<bool> response, string endpoint)
    {
        ThrowIfUnavailable(response, endpoint);
        if (response.Success)
        {
            return response.Data;
        }

        Debug.Warn($"{_moduleKey} WebAPI 业务失败: {endpoint}, {response.Message}");
        return false;
    }

    protected void ThrowIfUnavailable<TResponse>(ApiResponse<TResponse> response, string endpoint)
    {
        if (!response.IsTransportError)
        {
            return;
        }

        var inner = new InvalidOperationException(response.Message ?? "数据源不可达");
        throw new DataSourceUnavailableException(_moduleKey, endpoint, inner);
    }

    private static string BuildQueryString(QueryRequest req)
    {
        var parts = new List<string>
        {
            $"page={req.Page}",
            $"pageSize={req.PageSize}"
        };

        Add(parts, "keyword", req.Keyword);
        Add(parts, "sortBy", req.SortBy);
        parts.Add($"desc={req.Desc.ToString().ToLowerInvariant()}");

        if (req.Filters != null)
        {
            foreach (var (key, value) in req.Filters)
            {
                Add(parts, $"filters.{key}", value);
            }
        }

        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }

    private static void Add(List<string> parts, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        parts.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
    }
}
