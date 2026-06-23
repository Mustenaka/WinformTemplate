using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public sealed class LocalSysAccountRepository : LocalRepositoryBase<SysAccountModel>, ISysAccountRepository
{
    private const string SeedFile = "sysAccounts.json";

    public LocalSysAccountRepository() : base(SeedFile)
    {
    }

    public LocalSysAccountRepository(string seedRoot) : base(SeedFile, seedRoot)
    {
    }

    public Task<SysAccountModel?> GetByUsernameAsync(string username)
    {
        return Task.FromResult(Snapshot().FirstOrDefault(account =>
            string.Equals(account.SysAccountName, username, StringComparison.OrdinalIgnoreCase)));
    }

    public Task FreezeAccountAsync(long id)
    {
        SetAccountStatus(id, true);
        return Task.CompletedTask;
    }

    public Task UnfreezeAccountAsync(long id)
    {
        SetAccountStatus(id, false);
        return Task.CompletedTask;
    }

    protected override object? GetEntityId(SysAccountModel entity)
    {
        return entity.SysId;
    }

    protected override void SetEntityId(SysAccountModel entity, long id)
    {
        entity.SysId = id;
    }

    protected override IEnumerable<SysAccountModel> ApplyQuery(IEnumerable<SysAccountModel> query, QueryRequest req)
    {
        if (!string.IsNullOrWhiteSpace(req.Keyword))
        {
            var keyword = req.Keyword.Trim();
            query = query.Where(account =>
                TextContains(account.SysAccountName, keyword) ||
                TextContains(account.SysNickname, keyword));
        }

        return query;
    }

    protected override IEnumerable<SysAccountModel> ApplySorting(IEnumerable<SysAccountModel> query, QueryRequest req)
    {
        return string.IsNullOrWhiteSpace(req.SortBy)
            ? query.OrderBy(account => account.SysId)
            : base.ApplySorting(query, req);
    }

    private void SetAccountStatus(long id, bool status)
    {
        var changed = Write(items =>
        {
            var account = items.FirstOrDefault(item => item.SysId == id);
            if (account == null)
            {
                return false;
            }

            account.SysStatus = status;
            account.SysUpdateAt = DateTime.Now;
            return true;
        });

        if (!changed)
        {
            throw new InvalidOperationException($"Account {id} does not exist.");
        }
    }
}
