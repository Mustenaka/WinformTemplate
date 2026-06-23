using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Template.Context;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Template.Repositories;

public sealed class EfProductRepository : EfRepositoryBase<ProductModel>, IProductRepository
{
    private static readonly HashSet<string> SortableColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(ProductModel.Id),
        nameof(ProductModel.Name),
        nameof(ProductModel.Code),
        nameof(ProductModel.Price),
        nameof(ProductModel.Stock),
        nameof(ProductModel.Status),
        nameof(ProductModel.CreateAt),
        nameof(ProductModel.UpdateAt)
    };

    public EfProductRepository(TemplateDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task<ProductModel?> GetByIdAsync(object id)
    {
        var productId = Convert.ToInt64(id);
        return await DbSet
            .Include(product => product.Category)
            .FirstOrDefaultAsync(product => product.Id == productId);
    }

    public override async Task<PagedResult<ProductModel>> QueryAsync(QueryRequest req)
    {
        return await SearchProductsAsync(
            req.Keyword,
            TryLong(req.Filters, "categoryId"),
            TryInt(req.Filters, "status"),
            TryDecimal(req.Filters, "minPrice"),
            TryDecimal(req.Filters, "maxPrice"),
            TryDateTime(req.Filters, "startDate"),
            TryDateTime(req.Filters, "endDate"),
            req.Page,
            req.PageSize,
            req.SortBy,
            !req.Desc);
    }

    public override async Task<bool> UpdateAsync(ProductModel product)
    {
        var existing = await DbSet.FindAsync(product.Id);
        if (existing == null)
        {
            return false;
        }

        existing.Name = product.Name;
        existing.Code = product.Code;
        existing.CategoryId = product.CategoryId;
        existing.Price = product.Price;
        existing.Stock = product.Stock;
        existing.Status = product.Status;
        existing.Description = product.Description;
        existing.ImageUrl = product.ImageUrl;
        existing.Tags = product.Tags;
        existing.Reserved1 = product.Reserved1;
        existing.Reserved2 = product.Reserved2;
        existing.Reserved3 = product.Reserved3;
        existing.UpdateAt = DateTime.Now;

        return await DbContext.SaveChangesAsync() > 0;
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
        var query = ApplyProductFilters(DbSet.AsQueryable(), keyword, categoryId, status, minPrice, maxPrice, startDate, endDate);
        var totalCount = await query.CountAsync();

        query = ApplyProductSorting(query, orderBy, ascending);

        var items = await query
            .Skip((Math.Max(pageIndex, 1) - 1) * Math.Max(pageSize, 1))
            .Take(Math.Max(pageSize, 1))
            .Include(product => product.Category)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<ProductModel>
        {
            Items = items,
            Total = totalCount
        };
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
        return await ApplyProductFilters(DbSet.AsQueryable(), keyword, categoryId, status, minPrice, maxPrice, startDate, endDate)
            .Include(product => product.Category)
            .OrderByDescending(product => product.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> IsCodeExistsAsync(string code, long? excludeId = null)
    {
        var query = DbSet.Where(product => product.Code == code);
        if (excludeId.HasValue)
        {
            query = query.Where(product => product.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<int> GetCountByCategoryAsync(long categoryId)
    {
        return await DbSet.CountAsync(product => product.CategoryId == categoryId);
    }

    public async Task<int> DeleteByIdsAsync(IEnumerable<long> ids)
    {
        var idSet = ids.ToHashSet();
        var products = await DbSet.Where(product => idSet.Contains(product.Id)).ToListAsync();
        DbSet.RemoveRange(products);
        await DbContext.SaveChangesAsync();
        return products.Count;
    }

    public async Task<int> BatchUpdateStatusAsync(IEnumerable<long> ids, int status)
    {
        var idSet = ids.ToHashSet();
        var products = await DbSet.Where(product => idSet.Contains(product.Id)).ToListAsync();
        foreach (var product in products)
        {
            product.Status = status;
            product.UpdateAt = DateTime.Now;
        }

        await DbContext.SaveChangesAsync();
        return products.Count;
    }

    public async Task<int> BatchUpdateCategoryAsync(IEnumerable<long> ids, long categoryId)
    {
        var idSet = ids.ToHashSet();
        var products = await DbSet.Where(product => idSet.Contains(product.Id)).ToListAsync();
        foreach (var product in products)
        {
            product.CategoryId = categoryId;
            product.UpdateAt = DateTime.Now;
        }

        await DbContext.SaveChangesAsync();
        return products.Count;
    }

    private static IQueryable<ProductModel> ApplyProductFilters(
        IQueryable<ProductModel> query,
        string? keyword,
        long? categoryId,
        int? status,
        decimal? minPrice,
        decimal? maxPrice,
        DateTime? startDate,
        DateTime? endDate)
    {
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var trimmedKeyword = keyword.Trim();
            query = query.Where(product =>
                (product.Name != null && product.Name.Contains(trimmedKeyword)) ||
                (product.Code != null && product.Code.Contains(trimmedKeyword)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(product => product.Status == status.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(product => product.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(product => product.Price <= maxPrice.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(product => product.CreateAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var endOfDay = endDate.Value.Date.AddDays(1);
            query = query.Where(product => product.CreateAt < endOfDay);
        }

        return query;
    }

    private static IQueryable<ProductModel> ApplyProductSorting(IQueryable<ProductModel> query, string? orderBy, bool ascending)
    {
        return orderBy?.ToLowerInvariant() switch
        {
            "name" => ascending ? query.OrderBy(product => product.Name) : query.OrderByDescending(product => product.Name),
            "price" => ascending ? query.OrderBy(product => product.Price) : query.OrderByDescending(product => product.Price),
            "createat" => ascending ? query.OrderBy(product => product.CreateAt) : query.OrderByDescending(product => product.CreateAt),
            "stock" => ascending ? query.OrderBy(product => product.Stock) : query.OrderByDescending(product => product.Stock),
            "code" => ascending ? query.OrderBy(product => product.Code) : query.OrderByDescending(product => product.Code),
            _ => query.OrderByDescending(product => product.Id)
        };
    }

    private static long? TryLong(Dictionary<string, string>? filters, string key)
    {
        return filters != null && filters.TryGetValue(key, out var value) && long.TryParse(value, out var parsed)
            ? parsed
            : null;
    }

    private static int? TryInt(Dictionary<string, string>? filters, string key)
    {
        return filters != null && filters.TryGetValue(key, out var value) && int.TryParse(value, out var parsed)
            ? parsed
            : null;
    }

    private static decimal? TryDecimal(Dictionary<string, string>? filters, string key)
    {
        return filters != null && filters.TryGetValue(key, out var value) && decimal.TryParse(value, out var parsed)
            ? parsed
            : null;
    }

    private static DateTime? TryDateTime(Dictionary<string, string>? filters, string key)
    {
        return filters != null && filters.TryGetValue(key, out var value) && DateTime.TryParse(value, out var parsed)
            ? parsed
            : null;
    }

    protected override bool IsSortableColumn(string propertyName)
    {
        return SortableColumns.Contains(propertyName);
    }
}
