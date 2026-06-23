using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public class EfSysMenuRepository : EfRepositoryBase<SysMenuModel>, ISysMenuRepository
{
    private readonly SysDbContext _dbContext;

    public EfSysMenuRepository(SysDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SysMenuModel>> GetActiveMenusAsync()
    {
        return await DbSet
            .Where(menu => menu.SysStatus == false)
            .OrderBy(menu => menu.SmSort ?? int.MaxValue)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<SysMenuModel>> GetByParentIdAsync(long parentId)
    {
        return await DbSet
            .Where(menu => menu.SmParentId == parentId)
            .OrderBy(menu => menu.SmSort ?? int.MaxValue)
            .ToListAsync();
    }

    public override async Task<bool> UpdateAsync(SysMenuModel menu)
    {
        var existingMenu = await DbSet.FindAsync(menu.SmId);
        if (existingMenu == null)
        {
            return false;
        }

        existingMenu.SmParentId = menu.SmParentId;
        existingMenu.SmName = menu.SmName;
        existingMenu.SmEnName = menu.SmEnName;
        existingMenu.SmType = menu.SmType;
        existingMenu.SmUrl = menu.SmUrl;
        existingMenu.SmTarget = menu.SmTarget;
        existingMenu.SmLevel = menu.SmLevel;
        existingMenu.SmSort = menu.SmSort;
        existingMenu.SmIcon = menu.SmIcon;
        existingMenu.SmRemark = menu.SmRemark;
        existingMenu.SysStatus = menu.SysStatus;
        existingMenu.SysUpdateAt = DateTime.Now;
        existingMenu.SysReserved1 = menu.SysReserved1;
        existingMenu.SysReserved2 = menu.SysReserved2;
        existingMenu.SysReserved3 = menu.SysReserved3;

        return await DbContext.SaveChangesAsync() > 0;
    }

    public override async Task<bool> DeleteAsync(object id)
    {
        var menuId = Convert.ToInt64(id);
        var existingMenu = await DbSet.FindAsync(menuId);
        if (existingMenu == null)
        {
            return false;
        }

        var roleAuths = await _dbContext.SysRoleAuths
            .Where(roleAuth => roleAuth.SraMenuId == menuId)
            .ToListAsync();

        await using var transaction = await DbContext.Database.BeginTransactionAsync();
        _dbContext.SysRoleAuths.RemoveRange(roleAuths);
        DbSet.Remove(existingMenu);
        await DbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    public async Task FreezeMenuAsync(long id)
    {
        await SetMenuStatusAsync(id, true);
    }

    public async Task UnfreezeMenuAsync(long id)
    {
        await SetMenuStatusAsync(id, false);
    }

    protected override IQueryable<SysMenuModel> ApplyKeyword(IQueryable<SysMenuModel> query, string keyword)
    {
        return query.Where(menu =>
            menu.SmName.Contains(keyword) ||
            menu.SmEnName.Contains(keyword) ||
            menu.SmUrl.Contains(keyword));
    }

    protected override IQueryable<SysMenuModel> ApplySorting(IQueryable<SysMenuModel> query, QueryRequest req)
    {
        return string.IsNullOrWhiteSpace(req.SortBy)
            ? query.OrderBy(menu => menu.SmSort ?? int.MaxValue).ThenBy(menu => menu.SmId)
            : base.ApplySorting(query, req);
    }

    private async Task SetMenuStatusAsync(long id, bool status)
    {
        var existingMenu = await DbSet.FindAsync(id);
        if (existingMenu == null)
        {
            throw new InvalidOperationException($"菜单ID {id} 不存在");
        }

        existingMenu.SysStatus = status;
        existingMenu.SysUpdateAt = DateTime.Now;
        await DbContext.SaveChangesAsync();
    }
}
