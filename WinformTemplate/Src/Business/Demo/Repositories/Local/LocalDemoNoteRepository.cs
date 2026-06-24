using WinformTemplate.Business.Demo.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Demo.Repositories;

public sealed class LocalDemoNoteRepository : LocalRepositoryBase<DemoNote>, IDemoNoteRepository
{
    private const string SeedFile = "demoNotes.json";

    public LocalDemoNoteRepository() : base(SeedFile)
    {
    }

    public LocalDemoNoteRepository(string seedRoot) : base(SeedFile, seedRoot)
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

    public override Task<DemoNote> AddAsync(DemoNote entity)
    {
        entity.CreateAt = entity.CreateAt == default ? DateTime.Now : entity.CreateAt;
        entity.UpdateAt = DateTime.Now;
        return base.AddAsync(entity);
    }

    public override Task<bool> UpdateAsync(DemoNote entity)
    {
        entity.UpdateAt = DateTime.Now;
        return base.UpdateAsync(entity);
    }

    public Task<PagedResult<DemoNote>> SearchByTitleAsync(
        string? titleKeyword = null,
        int pageIndex = 1,
        int pageSize = 20,
        string? orderBy = null,
        bool ascending = false)
    {
        IEnumerable<DemoNote> query = Snapshot();
        if (!string.IsNullOrWhiteSpace(titleKeyword))
        {
            var keyword = titleKeyword.Trim();
            query = query.Where(note => TextContains(note.Title, keyword));
        }

        var total = query.Count();
        var items = ApplySorting(query, orderBy, ascending)
            .Skip((Math.Max(pageIndex, 1) - 1) * Math.Max(pageSize, 1))
            .Take(Math.Max(pageSize, 1))
            .ToList();

        return Task.FromResult(new PagedResult<DemoNote>
        {
            Items = items,
            Total = total
        });
    }

    protected override object? GetEntityId(DemoNote entity)
    {
        return entity.Id;
    }

    protected override void SetEntityId(DemoNote entity, long id)
    {
        entity.Id = id;
    }

    private static IOrderedEnumerable<DemoNote> ApplySorting(IEnumerable<DemoNote> query, string? orderBy, bool ascending)
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
