using WinformTemplate.Business.Template.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Template.Repositories;

public interface IImportRecordRepository : IRepository<ImportRecordModel>
{
    Task<List<ImportRecordModel>> GetRecentRecordsAsync(int count = 10);

    Task<List<ImportRecordModel>> GetRecordsByDateRangeAsync(DateTime startDate, DateTime endDate);

    Task<PagedResult<ImportRecordModel>> GetPagedRecordsAsync(int pageIndex = 1, int pageSize = 10);
}
