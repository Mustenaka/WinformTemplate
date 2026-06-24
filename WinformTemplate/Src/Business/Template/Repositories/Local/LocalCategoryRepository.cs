using WinformTemplate.Business.Template.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Template.Repositories;

public sealed class LocalCategoryRepository : LocalRepositoryBase<CategoryModel>, ICategoryRepository
{
    private const string SeedFile = "categories.json";

    public LocalCategoryRepository() : base(SeedFile)
    {
    }

    public LocalCategoryRepository(string seedRoot) : base(SeedFile, seedRoot)
    {
    }

    public Task<List<CategoryModel>> GetActiveCategoriesAsync()
    {
        return Task.FromResult(Snapshot()
            .Where(category => category.Status == 0)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ToList());
    }

    public Task<List<CategoryModel>> GetCategoryTreeAsync()
    {
        return Task.FromResult(Snapshot()
            .OrderBy(category => category.Level)
            .ThenBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ToList());
    }

    public Task<List<CategoryModel>> GetChildrenAsync(long? parentId)
    {
        return Task.FromResult(Snapshot()
            .Where(category => category.ParentId == parentId)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ToList());
    }

    public Task<bool> IsNameExistsAsync(string name, long? excludeId = null)
    {
        return Task.FromResult(Snapshot().Any(category =>
            string.Equals(category.Name, name, StringComparison.OrdinalIgnoreCase) &&
            (!excludeId.HasValue || category.Id != excludeId.Value)));
    }

    protected override object? GetEntityId(CategoryModel entity)
    {
        return entity.Id;
    }

    protected override void SetEntityId(CategoryModel entity, long id)
    {
        entity.Id = id;
    }

    protected override IEnumerable<CategoryModel> ApplyQuery(IEnumerable<CategoryModel> query, QueryRequest req)
    {
        if (!string.IsNullOrWhiteSpace(req.Keyword))
        {
            var keyword = req.Keyword.Trim();
            query = query.Where(category => TextContains(category.Name, keyword));
        }

        var status = TryInt(req.Filters, "status");
        if (status.HasValue)
        {
            query = query.Where(category => category.Status == status.Value);
        }

        var parentId = TryLong(req.Filters, "parentId");
        if (parentId.HasValue)
        {
            query = query.Where(category => category.ParentId == parentId.Value);
        }

        return query;
    }

    protected override IEnumerable<CategoryModel> ApplySorting(IEnumerable<CategoryModel> query, QueryRequest req)
    {
        return string.IsNullOrWhiteSpace(req.SortBy)
            ? query.OrderBy(category => category.SortOrder).ThenBy(category => category.Id)
            : base.ApplySorting(query, req);
    }
}
