using WinformTemplate.Business.Template.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Template.Repositories;

public sealed class LocalProductRepository : LocalRepositoryBase<ProductModel>, IProductRepository
{
    private const string SeedFile = "products.json";
    private readonly LocalCategoryRepository _categoryRepository;

    public LocalProductRepository(LocalCategoryRepository categoryRepository) : base(SeedFile)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
    }

    public LocalProductRepository(string seedRoot) : base(SeedFile, seedRoot)
    {
        _categoryRepository = new LocalCategoryRepository(seedRoot);
    }

    public override async Task<ProductModel?> GetByIdAsync(object id)
    {
        var product = await base.GetByIdAsync(id);
        AttachCategory(product);
        return product;
    }

    public override Task<PagedResult<ProductModel>> QueryAsync(QueryRequest req)
    {
        return SearchProductsAsync(
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

    public override async Task<ProductModel> AddAsync(ProductModel entity)
    {
        entity.CreateAt ??= DateTime.Now;
        entity.UpdateAt = DateTime.Now;
        var created = await base.AddAsync(entity);
        AttachCategory(created);
        return created;
    }

    public override Task<bool> UpdateAsync(ProductModel entity)
    {
        entity.UpdateAt = DateTime.Now;
        return base.UpdateAsync(entity);
    }

    public Task<PagedResult<ProductModel>> SearchProductsAsync(
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
        var query = ApplyProductFilters(Snapshot(), keyword, categoryId, status, minPrice, maxPrice, startDate, endDate);
        var total = query.Count();
        var items = ApplyProductSorting(query, orderBy, ascending)
            .Skip((Math.Max(pageIndex, 1) - 1) * Math.Max(pageSize, 1))
            .Take(Math.Max(pageSize, 1))
            .ToList();

        AttachCategories(items);
        return Task.FromResult(new PagedResult<ProductModel>
        {
            Items = items,
            Total = total
        });
    }

    public Task<List<ProductModel>> GetAllForExportAsync(
        string? keyword = null,
        long? categoryId = null,
        int? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var items = ApplyProductFilters(Snapshot(), keyword, categoryId, status, minPrice, maxPrice, startDate, endDate)
            .OrderByDescending(product => product.Id)
            .ToList();
        AttachCategories(items);
        return Task.FromResult(items);
    }

    public Task<bool> IsCodeExistsAsync(string code, long? excludeId = null)
    {
        return Task.FromResult(Snapshot().Any(product =>
            string.Equals(product.Code, code, StringComparison.OrdinalIgnoreCase) &&
            (!excludeId.HasValue || product.Id != excludeId.Value)));
    }

    public Task<int> GetCountByCategoryAsync(long categoryId)
    {
        return Task.FromResult(Snapshot().Count(product => product.CategoryId == categoryId));
    }

    public Task<int> DeleteByIdsAsync(IEnumerable<long> ids)
    {
        var idSet = ids.ToHashSet();
        return Task.FromResult(Write(items => items.RemoveAll(product => idSet.Contains(product.Id))));
    }

    public Task<int> BatchUpdateStatusAsync(IEnumerable<long> ids, int status)
    {
        var idSet = ids.ToHashSet();
        return Task.FromResult(Write(items =>
        {
            var count = 0;
            foreach (var product in items.Where(product => idSet.Contains(product.Id)))
            {
                product.Status = status;
                product.UpdateAt = DateTime.Now;
                count++;
            }

            return count;
        }));
    }

    public Task<int> BatchUpdateCategoryAsync(IEnumerable<long> ids, long categoryId)
    {
        var idSet = ids.ToHashSet();
        return Task.FromResult(Write(items =>
        {
            var count = 0;
            foreach (var product in items.Where(product => idSet.Contains(product.Id)))
            {
                product.CategoryId = categoryId;
                product.UpdateAt = DateTime.Now;
                count++;
            }

            return count;
        }));
    }

    protected override object? GetEntityId(ProductModel entity)
    {
        return entity.Id;
    }

    protected override void SetEntityId(ProductModel entity, long id)
    {
        entity.Id = id;
    }

    private static IEnumerable<ProductModel> ApplyProductFilters(
        IEnumerable<ProductModel> query,
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
                TextContains(product.Name, trimmedKeyword) ||
                TextContains(product.Code, trimmedKeyword));
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

    private static IOrderedEnumerable<ProductModel> ApplyProductSorting(IEnumerable<ProductModel> query, string? orderBy, bool ascending)
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

    private void AttachCategory(ProductModel? product)
    {
        if (product?.CategoryId == null)
        {
            return;
        }

        product.Category = _categoryRepository.GetByIdAsync(product.CategoryId.Value).GetAwaiter().GetResult();
    }

    private void AttachCategories(IEnumerable<ProductModel> products)
    {
        var categories = _categoryRepository.QueryAsync(new QueryRequest
        {
            Page = 1,
            PageSize = int.MaxValue
        }).GetAwaiter().GetResult().Items.ToDictionary(category => category.Id);

        foreach (var product in products)
        {
            if (product.CategoryId.HasValue && categories.TryGetValue(product.CategoryId.Value, out var category))
            {
                product.Category = category;
            }
        }
    }
}
