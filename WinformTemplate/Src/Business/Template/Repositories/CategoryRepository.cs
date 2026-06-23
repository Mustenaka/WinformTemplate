using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WinformTemplate.Business.Template.Context;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Business.Template.Repositories.Interface;

namespace WinformTemplate.Business.Template.Repositories;

/// <summary>
/// 分类仓储实现
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly TemplateDbContext _dbContext;
    private readonly DbSet<CategoryModel> _dbSet;

    public CategoryRepository(TemplateDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = _dbContext.Categories;
    }

    // ==================== 基础CRUD ====================

    public IQueryable<CategoryModel> GetAll()
    {
        return _dbSet;
    }

    public IQueryable<CategoryModel> GetByCondition(Expression<Func<CategoryModel, bool>> filter)
    {
        return _dbSet.Where(filter);
    }

    public async Task<CategoryModel?> GetByIdAsync(long id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task AddAsync(CategoryModel category)
    {
        await _dbSet.AddAsync(category);
    }

    public async Task AddRangeAsync(IEnumerable<CategoryModel> categories)
    {
        await _dbSet.AddRangeAsync(categories);
    }

    public void Update(CategoryModel category)
    {
        _dbContext.Entry(category).State = EntityState.Modified;
    }

    public void Delete(CategoryModel category)
    {
        _dbSet.Remove(category);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        var category = await _dbSet.FindAsync(id);
        if (category == null)
            return false;

        _dbSet.Remove(category);
        return true;
    }

    public async Task<bool> ExistsAsync(Expression<Func<CategoryModel, bool>> filter)
    {
        return await _dbSet.AnyAsync(filter);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    // ==================== 高级查询 ====================

    public async Task<List<CategoryModel>> GetActiveCategoriesAsync()
    {
        return await _dbSet
            .Where(c => c.Status == 0)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<CategoryModel>> GetCategoryTreeAsync()
    {
        return await _dbSet
            .OrderBy(c => c.Level)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<CategoryModel>> GetChildrenAsync(long? parentId)
    {
        return await _dbSet
            .Where(c => c.ParentId == parentId)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> IsNameExistsAsync(string name, long? excludeId = null)
    {
        var query = _dbSet.Where(c => c.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
