using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public class EfSysAccountRepository : EfRepositoryBase<SysAccountModel>, ISysAccountRepository
{
    public EfSysAccountRepository(SysDbContext dbContext) : base(dbContext)
    {
    }

    public Task<SysAccountModel?> GetByUsernameAsync(string username)
    {
        return DbSet.FirstOrDefaultAsync(account => account.SysAccountName == username);
    }

    public override async Task<bool> UpdateAsync(SysAccountModel account)
    {
        var existingAccount = await DbSet.FindAsync(account.SysId);
        if (existingAccount == null)
        {
            return false;
        }

        existingAccount.SysAccountName = account.SysAccountName;
        existingAccount.SysPassword = account.SysPassword;
        existingAccount.SysNickname = account.SysNickname;
        existingAccount.SysLevel = account.SysLevel;
        existingAccount.SysRoleId = account.SysRoleId;
        existingAccount.SysExtendId = account.SysExtendId;
        existingAccount.SysStatus = account.SysStatus;
        existingAccount.SysReserved1 = account.SysReserved1;
        existingAccount.SysReserved2 = account.SysReserved2;
        existingAccount.SysReserved3 = account.SysReserved3;
        existingAccount.SysUpdateAt = DateTime.Now;

        return await DbContext.SaveChangesAsync() > 0;
    }

    public async Task FreezeAccountAsync(long id)
    {
        await SetAccountStatusAsync(id, true);
    }

    public async Task UnfreezeAccountAsync(long id)
    {
        await SetAccountStatusAsync(id, false);
    }

    protected override IQueryable<SysAccountModel> ApplyKeyword(IQueryable<SysAccountModel> query, string keyword)
    {
        return query.Where(account =>
            (account.SysAccountName != null && account.SysAccountName.Contains(keyword)) ||
            (account.SysNickname != null && account.SysNickname.Contains(keyword)));
    }

    protected override IQueryable<SysAccountModel> ApplySorting(IQueryable<SysAccountModel> query, QueryRequest req)
    {
        return string.IsNullOrWhiteSpace(req.SortBy)
            ? query.OrderBy(account => account.SysId)
            : base.ApplySorting(query, req);
    }

    private async Task SetAccountStatusAsync(long id, bool status)
    {
        var existingAccount = await DbSet.FindAsync(id);
        if (existingAccount == null)
        {
            throw new InvalidOperationException($"用户ID {id} 不存在");
        }

        existingAccount.SysStatus = status;
        existingAccount.SysUpdateAt = DateTime.Now;
        await DbContext.SaveChangesAsync();
    }
}
