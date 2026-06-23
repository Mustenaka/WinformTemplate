using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Demo.Context;
using WinformTemplate.Business.Demo.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Demo.Repositories;

public sealed class EfDemoNoteRepository : EfRepositoryBase<DemoNote>, IDemoNoteRepository
{
    public EfDemoNoteRepository(DemoDbContext dbContext) : base(dbContext)
    {
    }

    public override Task<PagedResult<DemoNote>> QueryAsync(QueryRequest req)
    {
        return SearchByTitleAsync(
            req.Keyword,
            req.Page,
            req.PageSize,
            req.SortBy,
            !req.Desc);
    }

    public async Task<PagedResult<DemoNote>> SearchByTitleAsync(
        string? titleKeyword = null,
        int pageIndex = 1,
        int pageSize = 20,
        string? orderBy = null,
        bool ascending = false)
    {
        var query = DbSet.AsQueryable();
        if (!string.IsNullOrWhiteSpace(titleKeyword))
        {
            var keyword = titleKeyword.Trim();
            query = query.Where(note => note.Title.Contains(keyword));
        }

        var total = await query.CountAsync();
        var items = await ApplySorting(query, orderBy, ascending)
            .Skip((Math.Max(pageIndex, 1) - 1) * Math.Max(pageSize, 1))
            .Take(Math.Max(pageSize, 1))
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<DemoNote>
        {
            Items = items,
            Total = total
        };
    }

    public override async Task<bool> UpdateAsync(DemoNote entity)
    {
        var existing = await DbSet.FindAsync(entity.Id);
        if (existing == null)
        {
            return false;
        }

        existing.Title = entity.Title;
        existing.Content = entity.Content;
        existing.Pinned = entity.Pinned;

        return await DbContext.SaveChangesAsync() > 0;
    }

    private static IOrderedQueryable<DemoNote> ApplySorting(IQueryable<DemoNote> query, string? orderBy, bool ascending)
    {
        return orderBy?.ToLowerInvariant() switch
        {
            "id" => ascending ? query.OrderBy(note => note.Id) : query.OrderByDescending(note => note.Id),
            "title" => ascending ? query.OrderBy(note => note.Title) : query.OrderByDescending(note => note.Title),
            "pinned" => ascending ? query.OrderBy(note => note.Pinned) : query.OrderByDescending(note => note.Pinned),
            "updateat" => ascending ? query.OrderBy(note => note.UpdateAt) : query.OrderByDescending(note => note.UpdateAt),
            "createat" => ascending ? query.OrderBy(note => note.CreateAt) : query.OrderByDescending(note => note.CreateAt),
            _ => query.OrderByDescending(note => note.Pinned)
                .ThenByDescending(note => note.CreateAt)
                .ThenByDescending(note => note.Id)
        };
    }
}
