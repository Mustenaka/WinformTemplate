using System.Globalization;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Template.Repositories;

public sealed class ApiImportRecordRepository : ApiRepositoryBase<ImportRecordModel>, IImportRecordRepository
{
    public ApiImportRecordRepository(IWebApiClient client) : base(client, "Template", "import-records")
    {
    }

    public async Task<List<ImportRecordModel>> GetRecentRecordsAsync(int count = 10)
    {
        return (await GetListAsync<ImportRecordModel>($"{CollectionEndpoint}/recent{BuildQueryString(new[]
        {
            Pair("count", count.ToString(CultureInfo.InvariantCulture))
        })}")).ToList();
    }

    public async Task<List<ImportRecordModel>> GetRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return (await GetListAsync<ImportRecordModel>($"{CollectionEndpoint}/date-range{BuildQueryString(new[]
        {
            Pair("startDate", startDate.ToString("O", CultureInfo.InvariantCulture)),
            Pair("endDate", endDate.ToString("O", CultureInfo.InvariantCulture))
        })}")).ToList();
    }

    public async Task<PagedResult<ImportRecordModel>> GetPagedRecordsAsync(int pageIndex = 1, int pageSize = 10)
    {
        var endpoint = $"{CollectionEndpoint}{BuildQueryString(new QueryRequest
        {
            Page = pageIndex,
            PageSize = pageSize,
            SortBy = nameof(ImportRecordModel.CreateAt),
            Desc = true
        })}";

        return await GetOptionalAsync<PagedResult<ImportRecordModel>>(endpoint) ?? new PagedResult<ImportRecordModel>();
    }

    protected override object GetEntityId(ImportRecordModel entity)
    {
        return entity.Id;
    }

    private static KeyValuePair<string, string?> Pair(string key, string? value)
    {
        return new KeyValuePair<string, string?>(key, value);
    }
}
