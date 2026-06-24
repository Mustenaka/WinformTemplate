using WinformTemplate.Business.Demo.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Demo.Repositories;

public interface IDemoNoteRepository : IRepository<DemoNote>
{
    Task<PagedResult<DemoNote>> SearchByTitleAsync(
        string? titleKeyword = null,
        int pageIndex = 1,
        int pageSize = 20,
        string? orderBy = null,
        bool ascending = false);
}
