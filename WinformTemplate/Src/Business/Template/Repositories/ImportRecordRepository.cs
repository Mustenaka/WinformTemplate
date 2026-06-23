using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WinformTemplate.Business.Template.Context;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Business.Template.Repositories.Interface;

namespace WinformTemplate.Business.Template.Repositories;

/// <summary>
/// 导入记录仓储实现
/// </summary>
public class ImportRecordRepository : IImportRecordRepository
{
    private readonly TemplateDbContext _dbContext;
    private readonly DbSet<ImportRecordModel> _dbSet;

    public ImportRecordRepository(TemplateDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = _dbContext.ImportRecords;
    }

    // ==================== 基础CRUD ====================

    public IQueryable<ImportRecordModel> GetAll()
    {
        return _dbSet;
    }

    public IQueryable<ImportRecordModel> GetByCondition(Expression<Func<ImportRecordModel, bool>> filter)
    {
        return _dbSet.Where(filter);
    }

    public async Task<ImportRecordModel?> GetByIdAsync(long id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task AddAsync(ImportRecordModel record)
    {
        await _dbSet.AddAsync(record);
    }

    public void Update(ImportRecordModel record)
    {
        _dbContext.Entry(record).State = EntityState.Modified;
    }

    public void Delete(ImportRecordModel record)
    {
        _dbSet.Remove(record);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    // ==================== 高级查询 ====================

    public async Task<List<ImportRecordModel>> GetRecentRecordsAsync(int count = 10)
    {
        return await _dbSet
            .OrderByDescending(r => r.CreateAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<ImportRecordModel>> GetRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var endOfDay = endDate.Date.AddDays(1);

        return await _dbSet
            .Where(r => r.CreateAt >= startDate && r.CreateAt < endOfDay)
            .OrderByDescending(r => r.CreateAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(List<ImportRecordModel> Items, int TotalCount)> GetPagedRecordsAsync(
        int pageIndex = 1,
        int pageSize = 10)
    {
        var query = _dbSet.AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CreateAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (items, totalCount);
    }
}
