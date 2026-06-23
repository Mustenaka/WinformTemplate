using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public interface ISysAccountRepository : IRepository<SysAccountModel>
{
    Task<SysAccountModel?> GetByUsernameAsync(string username);

    Task FreezeAccountAsync(long id);

    Task UnfreezeAccountAsync(long id);
}
