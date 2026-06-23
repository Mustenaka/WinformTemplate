using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WinformTemplate.Business.Template.Context;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Business.Template.Repositories.Interface;

namespace WinformTemplate.Business.Template.Repositories;

/// <summary>
/// 产品仓储实现
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly TemplateDbContext _dbContext;
    private readonly DbSet<ProductModel> _dbSet;

    public ProductRepository(TemplateDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = _dbContext.Products;
    }

    // ==================== 基础CRUD ====================

    public IQueryable<ProductModel> GetAll()
    {
        return _dbSet.Include(p => p.Category);
    }

    public IQueryable<ProductModel> GetByCondition(Expression<Func<ProductModel, bool>> filter)
    {
        return _dbSet.Where(filter).Include(p => p.Category);
    }

    public async Task<ProductModel?> GetByIdAsync(long id)
    {
        return await _dbSet.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(ProductModel product)
    {
        await _dbSet.AddAsync(product);
    }

    public async Task AddRangeAsync(IEnumerable<ProductModel> products)
    {
        await _dbSet.AddRangeAsync(products);
    }

    public void Update(ProductModel product)
    {
        _dbContext.Entry(product).State = EntityState.Modified;
    }

    public void UpdateRange(IEnumerable<ProductModel> products)
    {
        foreach (var product in products)
        {
            _dbContext.Entry(product).State = EntityState.Modified;
        }
    }

    public void Delete(ProductModel product)
    {
        _dbSet.Remove(product);
    }

    public void DeleteRange(IEnumerable<ProductModel> products)
    {
        _dbSet.RemoveRange(products);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        var product = await _dbSet.FindAsync(id);
        if (product == null)
            return false;

        _dbSet.Remove(product);
        return true;
    }

    public async Task<int> DeleteByIdsAsync(IEnumerable<long> ids)
    {
        var products = await _dbSet.Where(p => ids.Contains(p.Id)).ToListAsync();
        if (!products.Any())
            return 0;

        _dbSet.RemoveRange(products);
        return products.Count;
    }

    public async Task<bool> ExistsAsync(Expression<Func<ProductModel, bool>> filter)
    {
        return await _dbSet.AnyAsync(filter);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    // ==================== 高级查询 ====================

    public async Task<(List<ProductModel> Items, int TotalCount)> SearchProductsAsync(
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
        var query = _dbSet.AsQueryable();

        // 多条件筛选
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.Trim();
            query = query.Where(p =>
                (p.Name != null && p.Name.Contains(keyword)) ||
                (p.Code != null && p.Code.Contains(keyword)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(p => p.CreateAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            // 包含结束日期当天的所有数据
            var endOfDay = endDate.Value.Date.AddDays(1);
            query = query.Where(p => p.CreateAt < endOfDay);
        }

        // 计算总数（在排序和分页之前）
        var totalCount = await query.CountAsync();

        // 排序
        query = orderBy?.ToLower() switch
        {
            "name" => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            "price" => ascending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
            "createat" => ascending ? query.OrderBy(p => p.CreateAt) : query.OrderByDescending(p => p.CreateAt),
            "stock" => ascending ? query.OrderBy(p => p.Stock) : query.OrderByDescending(p => p.Stock),
            "code" => ascending ? query.OrderBy(p => p.Code) : query.OrderByDescending(p => p.Code),
            _ => query.OrderByDescending(p => p.Id) // 默认按ID降序
        };

        // 分页
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.Category)
            .AsNoTracking() // 只读查询，提升性能
            .ToListAsync();

        return (items, totalCount);
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
        var query = _dbSet.AsQueryable();

        // 应用相同的筛选条件
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.Trim();
            query = query.Where(p =>
                (p.Name != null && p.Name.Contains(keyword)) ||
                (p.Code != null && p.Code.Contains(keyword)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(p => p.CreateAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var endOfDay = endDate.Value.Date.AddDays(1);
            query = query.Where(p => p.CreateAt < endOfDay);
        }

        return await query
            .Include(p => p.Category)
            .OrderByDescending(p => p.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> IsCodeExistsAsync(string code, long? excludeId = null)
    {
        var query = _dbSet.Where(p => p.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<int> GetCountByCategoryAsync(long categoryId)
    {
        return await _dbSet.CountAsync(p => p.CategoryId == categoryId);
    }

    public async Task<int> BatchUpdateStatusAsync(IEnumerable<long> ids, int status)
    {
        var products = await _dbSet.Where(p => ids.Contains(p.Id)).ToListAsync();

        foreach (var product in products)
        {
            product.Status = status;
        }

        return products.Count;
    }

    public async Task<int> BatchUpdateCategoryAsync(IEnumerable<long> ids, long categoryId)
    {
        var products = await _dbSet.Where(p => ids.Contains(p.Id)).ToListAsync();

        foreach (var product in products)
        {
            product.CategoryId = categoryId;
        }

        return products.Count;
    }
}
