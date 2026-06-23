using System.Globalization;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Template.Repositories;

public sealed class ApiProductRepository : ApiRepositoryBase<ProductModel>, IProductRepository
{
    public ApiProductRepository(IWebApiClient client) : base(client, "Template", "products")
    {
    }

    public async Task<PagedResult<ProductModel>> SearchProductsAsync(
        string? keyword = null,
        long? categoryId = null,
        int? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageIndex = 1,
        int pageSize = 10,
        string? orderBy = null,
        bool ascending = true)
    {
        var endpoint = $"{CollectionEndpoint}{BuildProductQueryString(keyword, categoryId, status, minPrice, maxPrice, startDate, endDate, pageIndex, pageSize, orderBy, ascending)}";
        return await GetOptionalAsync<PagedResult<ProductModel>>(endpoint) ?? new PagedResult<ProductModel>();
    }

    public async Task<List<ProductModel>> GetAllForExportAsync(
        string? keyword = null,
        long? categoryId = null,
        int? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var endpoint = $"{CollectionEndpoint}/export{BuildProductQueryString(keyword, categoryId, status, minPrice, maxPrice, startDate, endDate, null, null, null, true)}";
        return (await GetListAsync<ProductModel>(endpoint)).ToList();
    }

    public Task<bool> IsCodeExistsAsync(string code, long? excludeId = null)
    {
        var endpoint = $"{CollectionEndpoint}/code-exists{BuildQueryString(new[]
        {
            Pair("code", code),
            Pair("excludeId", ToString(excludeId))
        })}";
        return GetBooleanAsync(endpoint);
    }

    public async Task<int> GetCountByCategoryAsync(long categoryId)
    {
        return await GetOptionalAsync<int>($"{CollectionEndpoint}/count-by-category/{Escape(categoryId)}");
    }

    public async Task<int> DeleteByIdsAsync(IEnumerable<long> ids)
    {
        return await PostOptionalAsync<int>($"{CollectionEndpoint}/batch-delete", new { ids = ids.ToArray() });
    }

    public async Task<int> BatchUpdateStatusAsync(IEnumerable<long> ids, int status)
    {
        return await PostOptionalAsync<int>($"{CollectionEndpoint}/batch-status", new { ids = ids.ToArray(), status });
    }

    public async Task<int> BatchUpdateCategoryAsync(IEnumerable<long> ids, long categoryId)
    {
        return await PostOptionalAsync<int>($"{CollectionEndpoint}/batch-category", new { ids = ids.ToArray(), categoryId });
    }

    protected override object GetEntityId(ProductModel entity)
    {
        return entity.Id;
    }

    private static string BuildProductQueryString(
        string? keyword,
        long? categoryId,
        int? status,
        decimal? minPrice,
        decimal? maxPrice,
        DateTime? startDate,
        DateTime? endDate,
        int? pageIndex,
        int? pageSize,
        string? orderBy,
        bool ascending)
    {
        return BuildQueryString(new[]
        {
            Pair("page", ToString(pageIndex)),
            Pair("pageSize", ToString(pageSize)),
            Pair("keyword", keyword),
            Pair("filters.categoryId", ToString(categoryId)),
            Pair("filters.status", ToString(status)),
            Pair("filters.minPrice", ToString(minPrice)),
            Pair("filters.maxPrice", ToString(maxPrice)),
            Pair("filters.startDate", ToString(startDate)),
            Pair("filters.endDate", ToString(endDate)),
            Pair("sortBy", orderBy),
            Pair("desc", (!ascending).ToString().ToLowerInvariant())
        });
    }

    private static KeyValuePair<string, string?> Pair(string key, string? value)
    {
        return new KeyValuePair<string, string?>(key, value);
    }

    private static string? ToString<TValue>(TValue? value) where TValue : struct
    {
        return value switch
        {
            null => null,
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            decimal decimalValue => decimalValue.ToString(CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture)
        };
    }
}
