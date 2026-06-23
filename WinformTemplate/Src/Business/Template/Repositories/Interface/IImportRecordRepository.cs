using System.Linq.Expressions;
using WinformTemplate.Business.Template.Model;

namespace WinformTemplate.Business.Template.Repositories.Interface;

/// <summary>
/// 导入记录仓储接口
/// </summary>
public interface IImportRecordRepository
{
    // ==================== 基础CRUD ====================

    IQueryable<ImportRecordModel> GetAll();

    IQueryable<ImportRecordModel> GetByCondition(Expression<Func<ImportRecordModel, bool>> filter);

    Task<ImportRecordModel?> GetByIdAsync(long id);

    Task AddAsync(ImportRecordModel record);

    void Update(ImportRecordModel record);

    void Delete(ImportRecordModel record);

    Task<int> SaveChangesAsync();

    // ==================== 高级查询 ====================

    /// <summary>
    /// 获取最近的导入记录
    /// </summary>
    Task<List<ImportRecordModel>> GetRecentRecordsAsync(int count = 10);

    /// <summary>
    /// 根据日期范围获取导入记录
    /// </summary>
    Task<List<ImportRecordModel>> GetRecordsByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 获取导入记录（支持分页）
    /// </summary>
    Task<(List<ImportRecordModel> Items, int TotalCount)> GetPagedRecordsAsync(
        int pageIndex = 1,
        int pageSize = 10);
}
