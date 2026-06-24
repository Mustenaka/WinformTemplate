using WinformTemplate.Business.Template.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Template.Repositories;

public sealed class ApiCategoryRepository : ApiRepositoryBase<CategoryModel>, ICategoryRepository
{
    public ApiCategoryRepository(IWebApiClient client) : base(client, "Template", "categories")
    {
    }

    public async Task<List<CategoryModel>> GetActiveCategoriesAsync()
    {
        return (await GetListAsync<CategoryModel>($"{CollectionEndpoint}/active")).ToList();
    }

    public async Task<List<CategoryModel>> GetCategoryTreeAsync()
    {
        return (await GetListAsync<CategoryModel>($"{CollectionEndpoint}/tree")).ToList();
    }

    public async Task<List<CategoryModel>> GetChildrenAsync(long? parentId)
    {
        var endpoint = $"{CollectionEndpoint}/children{BuildQueryString(new[]
        {
            Pair("parentId", parentId?.ToString())
        })}";
        return (await GetListAsync<CategoryModel>(endpoint)).ToList();
    }

    public Task<bool> IsNameExistsAsync(string name, long? excludeId = null)
    {
        var endpoint = $"{CollectionEndpoint}/name-exists{BuildQueryString(new[]
        {
            Pair("name", name),
            Pair("excludeId", excludeId?.ToString())
        })}";
        return GetBooleanAsync(endpoint);
    }

    protected override object GetEntityId(CategoryModel entity)
    {
        return entity.Id;
    }

    private static KeyValuePair<string, string?> Pair(string key, string? value)
    {
        return new KeyValuePair<string, string?>(key, value);
    }
}
