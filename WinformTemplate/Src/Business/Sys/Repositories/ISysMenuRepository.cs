using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public interface ISysMenuRepository : IRepository<SysMenuModel>
{
    Task<IReadOnlyList<SysMenuModel>> GetActiveMenusAsync();

    Task<IReadOnlyList<SysMenuModel>> GetByParentIdAsync(long parentId);

    Task FreezeMenuAsync(long id);

    Task UnfreezeMenuAsync(long id);
}
