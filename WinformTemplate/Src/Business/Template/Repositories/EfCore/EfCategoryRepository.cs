using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Template.Context;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Template.Repositories;

public sealed class EfCategoryRepository : EfRepositoryBase<CategoryModel>, ICategoryRepository
{
    public EfCategoryRepository(TemplateDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task<bool> UpdateAsync(CategoryModel category)
    {
        var existing = await DbSet.FindAsync(category.Id);
        if (existing == null)
        {
            return false;
        }

        existing.Name = category.Name;
        existing.ParentId = category.ParentId;
        existing.Level = category.Level;
        existing.SortOrder = category.SortOrder;
        existing.Status = category.Status;
        existing.UpdateAt = DateTime.Now;

        return await DbContext.SaveChangesAsync() > 0;
    }

    public async Task<List<CategoryModel>> GetActiveCategoriesAsync()
    {
        return await DbSet
            .Where(category => category.Status == 0)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<CategoryModel>> GetCategoryTreeAsync()
    {
        return await DbSet
            .OrderBy(category => category.Level)
            .ThenBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<CategoryModel>> GetChildrenAsync(long? parentId)
    {
        return await DbSet
            .Where(category => category.ParentId == parentId)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> IsNameExistsAsync(string name, long? excludeId = null)
    {
        var query = DbSet.Where(category => category.Name == name);
        if (excludeId.HasValue)
        {
            query = query.Where(category => category.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    protected override IQueryable<CategoryModel> ApplyKeyword(IQueryable<CategoryModel> query, string keyword)
    {
        return query.Where(category => category.Name != null && category.Name.Contains(keyword));
    }

    protected override IQueryable<CategoryModel> ApplyFilters(IQueryable<CategoryModel> query, IReadOnlyDictionary<string, string> filters)
    {
        if (filters.TryGetValue("status", out var statusText) && int.TryParse(statusText, out var status))
        {
            query = query.Where(category => category.Status == status);
        }

        if (filters.TryGetValue("parentId", out var parentIdText) && long.TryParse(parentIdText, out var parentId))
        {
            query = query.Where(category => category.ParentId == parentId);
        }

        return query;
    }

    protected override IQueryable<CategoryModel> ApplySorting(IQueryable<CategoryModel> query, QueryRequest req)
    {
        return string.IsNullOrWhiteSpace(req.SortBy)
            ? query.OrderBy(category => category.SortOrder).ThenBy(category => category.Id)
            : base.ApplySorting(query, req);
    }
}
