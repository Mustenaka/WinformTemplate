using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Template.Context;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Template.Repositories;

public sealed class EfImportRecordRepository : EfRepositoryBase<ImportRecordModel>, IImportRecordRepository
{
    public EfImportRecordRepository(TemplateDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<ImportRecordModel>> GetRecentRecordsAsync(int count = 10)
    {
        return await DbSet
            .OrderByDescending(record => record.CreateAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<ImportRecordModel>> GetRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var endOfDay = endDate.Date.AddDays(1);

        return await DbSet
            .Where(record => record.CreateAt >= startDate && record.CreateAt < endOfDay)
            .OrderByDescending(record => record.CreateAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<PagedResult<ImportRecordModel>> GetPagedRecordsAsync(int pageIndex = 1, int pageSize = 10)
    {
        return await QueryAsync(new QueryRequest
        {
            Page = pageIndex,
            PageSize = pageSize,
            SortBy = nameof(ImportRecordModel.CreateAt),
            Desc = true
        });
    }

    protected override IQueryable<ImportRecordModel> ApplySorting(IQueryable<ImportRecordModel> query, QueryRequest req)
    {
        return string.IsNullOrWhiteSpace(req.SortBy)
            ? query.OrderByDescending(record => record.CreateAt)
            : base.ApplySorting(query, req);
    }
}
