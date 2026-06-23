using System.Globalization;
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
        var endpoint = $"{CollectionEndpoint}/{Escape(id)}";
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
            throw new InvalidOperationException(response.Message ?? "Add operation failed.");
        }

        return created;
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        var endpoint = $"{CollectionEndpoint}/{Escape(GetEntityId(entity))}";
        var response = await _client.PutAsync<bool>(endpoint, entity);
        return HandleBooleanResponse(response, endpoint);
    }

    public virtual async Task<bool> DeleteAsync(object id)
    {
        var endpoint = $"{CollectionEndpoint}/{Escape(id)}";
        var response = await _client.DeleteAsync<bool>(endpoint);
        return HandleBooleanResponse(response, endpoint);
    }

    protected abstract object GetEntityId(T entity);

    protected async Task<TResponse?> GetOptionalAsync<TResponse>(string endpoint)
    {
        var response = await _client.GetAsync<TResponse>(endpoint);
        return HandleOptionalResponse(response, endpoint);
    }

    protected async Task<bool> GetBooleanAsync(string endpoint)
    {
        var response = await _client.GetAsync<bool>(endpoint);
        return HandleBooleanResponse(response, endpoint);
    }

    protected async Task<IReadOnlyList<TResponse>> GetListAsync<TResponse>(string endpoint)
    {
        var response = await _client.GetAsync<List<TResponse>>(endpoint);
        var items = HandleOptionalResponse(response, endpoint);
        return items is not null ? items : Array.Empty<TResponse>();
    }

    protected async Task<TResponse?> PostOptionalAsync<TResponse>(string endpoint, object body)
    {
        var response = await _client.PostAsync<TResponse>(endpoint, body);
        return HandleOptionalResponse(response, endpoint);
    }

    protected async Task<bool> PostBooleanAsync(string endpoint, object body)
    {
        var response = await _client.PostAsync<bool>(endpoint, body);
        return HandleBooleanResponse(response, endpoint);
    }

    protected async Task<bool> PutBooleanAsync(string endpoint, object body)
    {
        var response = await _client.PutAsync<bool>(endpoint, body);
        return HandleBooleanResponse(response, endpoint);
    }

    protected async Task<bool> DeleteBooleanAsync(string endpoint)
    {
        var response = await _client.DeleteAsync<bool>(endpoint);
        return HandleBooleanResponse(response, endpoint);
    }

    protected TResponse? HandleOptionalResponse<TResponse>(ApiResponse<TResponse> response, string endpoint)
    {
        ThrowIfUnavailable(response, endpoint);
        if (response.Success)
        {
            return response.Data;
        }

        Debug.Warn($"{_moduleKey} WebAPI business failed: {endpoint}, {response.Message}");
        return default;
    }

    protected bool HandleBooleanResponse(ApiResponse<bool> response, string endpoint)
    {
        ThrowIfUnavailable(response, endpoint);
        if (response.Success)
        {
            return response.Data;
        }

        Debug.Warn($"{_moduleKey} WebAPI business failed: {endpoint}, {response.Message}");
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

    protected static string BuildQueryString(QueryRequest req)
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

    protected static string BuildQueryString(IEnumerable<KeyValuePair<string, string?>> parameters)
    {
        var parts = new List<string>();
        foreach (var (key, value) in parameters)
        {
            Add(parts, key, value);
        }

        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }

    protected static string Escape(object? value)
    {
        return Uri.EscapeDataString(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
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
