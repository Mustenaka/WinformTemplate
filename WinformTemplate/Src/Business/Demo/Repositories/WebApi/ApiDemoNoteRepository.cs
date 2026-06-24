using WinformTemplate.Business.Demo.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Demo.Repositories;

public sealed class ApiDemoNoteRepository : ApiRepositoryBase<DemoNote>, IDemoNoteRepository
{
    public ApiDemoNoteRepository(IWebApiClient client) : base(client, "Demo", "notes")
    {
    }

    public Task<PagedResult<DemoNote>> SearchByTitleAsync(
        string? titleKeyword = null,
        int pageIndex = 1,
        int pageSize = 20,
        string? orderBy = null,
        bool ascending = false)
    {
        return QueryAsync(new QueryRequest
        {
            Page = pageIndex,
            PageSize = pageSize,
            Keyword = titleKeyword,
            SortBy = orderBy,
            Desc = !ascending
        });
    }

    protected override object GetEntityId(DemoNote entity)
    {
        return entity.Id;
    }
}
