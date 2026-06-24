using WinformTemplate.Business.Template.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Template.Repositories;

public sealed class LocalImportRecordRepository : LocalRepositoryBase<ImportRecordModel>, IImportRecordRepository
{
    private const string SeedFile = "importRecords.json";

    public LocalImportRecordRepository() : base(SeedFile)
    {
    }

    public LocalImportRecordRepository(string seedRoot) : base(SeedFile, seedRoot)
    {
    }

    public Task<List<ImportRecordModel>> GetRecentRecordsAsync(int count = 10)
    {
        return Task.FromResult(Snapshot()
            .OrderByDescending(record => record.CreateAt)
            .Take(count)
            .ToList());
    }

    public Task<List<ImportRecordModel>> GetRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var endOfDay = endDate.Date.AddDays(1);
        return Task.FromResult(Snapshot()
            .Where(record => record.CreateAt >= startDate && record.CreateAt < endOfDay)
            .OrderByDescending(record => record.CreateAt)
            .ToList());
    }

    public Task<PagedResult<ImportRecordModel>> GetPagedRecordsAsync(int pageIndex = 1, int pageSize = 10)
    {
        return QueryAsync(new QueryRequest
        {
            Page = pageIndex,
            PageSize = pageSize,
            SortBy = nameof(ImportRecordModel.CreateAt),
            Desc = true
        });
    }

    protected override object? GetEntityId(ImportRecordModel entity)
    {
        return entity.Id;
    }

    protected override void SetEntityId(ImportRecordModel entity, long id)
    {
        entity.Id = id;
    }

    protected override IEnumerable<ImportRecordModel> ApplySorting(IEnumerable<ImportRecordModel> query, QueryRequest req)
    {
        return string.IsNullOrWhiteSpace(req.SortBy)
            ? query.OrderByDescending(record => record.CreateAt)
            : base.ApplySorting(query, req);
    }
}
