using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace WinformTemplate.Common.DataAccess;

public abstract class EfRepositoryBase<T> : IRepository<T> where T : class
{
    protected EfRepositoryBase(DbContext dbContext)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        DbSet = DbContext.Set<T>();
    }

    protected DbContext DbContext { get; }

    protected DbSet<T> DbSet { get; }

    public virtual async Task<T?> GetByIdAsync(object id)
    {
        return await DbSet.FindAsync(id);
    }

    public virtual async Task<PagedResult<T>> QueryAsync(QueryRequest req)
    {
        ArgumentNullException.ThrowIfNull(req);

        var page = Math.Max(req.Page, 1);
        var pageSize = Math.Max(req.PageSize, 1);
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
        {
            query = ApplyKeyword(query, req.Keyword.Trim());
        }

        if (req.Filters is { Count: > 0 })
        {
            query = ApplyFilters(query, req.Filters);
        }

        var total = await query.CountAsync();
        query = ApplySorting(query, req);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            Total = total
        };
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
        await DbContext.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        DbContext.Entry(entity).State = EntityState.Modified;
        return await DbContext.SaveChangesAsync() > 0;
    }

    public virtual async Task<bool> DeleteAsync(object id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
        {
            return false;
        }

        DbSet.Remove(entity);
        return await DbContext.SaveChangesAsync() > 0;
    }

    protected virtual IQueryable<T> ApplyKeyword(IQueryable<T> query, string keyword)
    {
        return query;
    }

    protected virtual IQueryable<T> ApplyFilters(IQueryable<T> query, IReadOnlyDictionary<string, string> filters)
    {
        return query;
    }

    protected virtual IQueryable<T> ApplySorting(IQueryable<T> query, QueryRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.SortBy))
        {
            return query;
        }

        var property = typeof(T).GetProperty(
            req.SortBy.Trim(),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (property == null || !IsSortableColumn(property.Name))
        {
            return query;
        }

        return req.Desc
            ? query.OrderByDescending(entity => EF.Property<object>(entity, property.Name))
            : query.OrderBy(entity => EF.Property<object>(entity, property.Name));
    }

    protected virtual bool IsSortableColumn(string propertyName)
    {
        return true;
    }
}
