using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public class EfSysRoleRepository : EfRepositoryBase<SysRoleModel>, ISysRoleRepository
{
    private readonly SysDbContext _dbContext;

    public EfSysRoleRepository(SysDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SysMenuModel>> GetMenusByRoleIdAsync(long roleId)
    {
        return await _dbContext.SysRoleAuths
            .Where(roleAuth => roleAuth.SraRoleId == roleId && roleAuth.Menu.SysStatus == false)
            .Select(roleAuth => roleAuth.Menu)
            .OrderBy(menu => menu.SmSort ?? int.MaxValue)
            .ToListAsync();
    }

    public Task<bool> HasMenuPermissionAsync(long roleId, long menuId)
    {
        return _dbContext.SysRoleAuths
            .AnyAsync(roleAuth => roleAuth.SraRoleId == roleId && roleAuth.SraMenuId == menuId);
    }

    public override async Task<bool> UpdateAsync(SysRoleModel role)
    {
        var existingRole = await DbSet.FindAsync(role.SrId);
        if (existingRole == null)
        {
            return false;
        }

        existingRole.SrName = role.SrName;
        existingRole.SrEnName = role.SrEnName;
        existingRole.SrRemark = role.SrRemark;
        existingRole.SrStatus = role.SrStatus;
        existingRole.SrUpdateAt = DateTime.Now;

        return await DbContext.SaveChangesAsync() > 0;
    }

    public override async Task<bool> DeleteAsync(object id)
    {
        var roleId = Convert.ToInt64(id);
        var role = await DbSet.FindAsync(roleId);
        if (role == null)
        {
            return false;
        }

        var roleAuths = await _dbContext.SysRoleAuths
            .Where(roleAuth => roleAuth.SraRoleId == roleId)
            .ToListAsync();

        await using var transaction = await DbContext.Database.BeginTransactionAsync();
        _dbContext.SysRoleAuths.RemoveRange(roleAuths);
        DbSet.Remove(role);
        await DbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    public async Task AssignMenuToRoleAsync(long roleId, long menuId)
    {
        if (await HasMenuPermissionAsync(roleId, menuId))
        {
            return;
        }

        await _dbContext.SysRoleAuths.AddAsync(new SysRoleAuthModel
        {
            SraRoleId = roleId,
            SraMenuId = menuId
        });
        await DbContext.SaveChangesAsync();
    }

    public async Task RemoveMenuFromRoleAsync(long roleId, long menuId)
    {
        var roleAuth = await _dbContext.SysRoleAuths
            .FirstOrDefaultAsync(item => item.SraRoleId == roleId && item.SraMenuId == menuId);

        if (roleAuth == null)
        {
            return;
        }

        _dbContext.SysRoleAuths.Remove(roleAuth);
        await DbContext.SaveChangesAsync();
    }

    protected override IQueryable<SysRoleModel> ApplyKeyword(IQueryable<SysRoleModel> query, string keyword)
    {
        return query.Where(role =>
            role.SrName.Contains(keyword) ||
            role.SrEnName.Contains(keyword) ||
            role.SrRemark.Contains(keyword));
    }

    protected override IQueryable<SysRoleModel> ApplySorting(IQueryable<SysRoleModel> query, QueryRequest req)
    {
        return string.IsNullOrWhiteSpace(req.SortBy)
            ? query.OrderBy(role => role.SrId)
            : base.ApplySorting(query, req);
    }
}
