using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Sys.Repositories;

public sealed class LocalSysRoleRepository : LocalRepositoryBase<SysRoleModel>, ISysRoleRepository
{
    private const string SeedFile = "sysRoles.json";
    private readonly LocalSysMenuRepository _menuRepository;
    private readonly LocalSysRoleAuthRepository _roleAuthRepository;

    public LocalSysRoleRepository(
        LocalSysMenuRepository menuRepository,
        LocalSysRoleAuthRepository roleAuthRepository) : base(SeedFile)
    {
        _menuRepository = menuRepository ?? throw new ArgumentNullException(nameof(menuRepository));
        _roleAuthRepository = roleAuthRepository ?? throw new ArgumentNullException(nameof(roleAuthRepository));
    }

    public LocalSysRoleRepository(string seedRoot) : base(SeedFile, seedRoot)
    {
        _menuRepository = new LocalSysMenuRepository(seedRoot);
        _roleAuthRepository = new LocalSysRoleAuthRepository(seedRoot);
    }

    public async Task<List<SysMenuModel>> GetMenusByRoleIdAsync(long roleId)
    {
        var roleAuths = await _roleAuthRepository.GetByRoleIdAsync(roleId);
        var menuIds = roleAuths.Select(roleAuth => roleAuth.SraMenuId).ToHashSet();
        var menus = await _menuRepository.QueryAsync(new QueryRequest
        {
            Page = 1,
            PageSize = int.MaxValue
        });

        return menus.Items
            .Where(menu => menuIds.Contains(menu.SmId) && menu.SysStatus == false)
            .OrderBy(menu => menu.SmSort ?? int.MaxValue)
            .ThenBy(menu => menu.SmId)
            .ToList();
    }

    public Task<bool> HasMenuPermissionAsync(long roleId, long menuId)
    {
        return _roleAuthRepository.HasPermissionAsync(roleId, menuId);
    }

    public async Task AssignMenuToRoleAsync(long roleId, long menuId)
    {
        await _roleAuthRepository.AssignPermissionAsync(roleId, menuId);
    }

    public async Task RemoveMenuFromRoleAsync(long roleId, long menuId)
    {
        await _roleAuthRepository.RemovePermissionAsync(roleId, menuId);
    }

    public override async Task<bool> DeleteAsync(object id)
    {
        var roleId = Convert.ToInt64(id);
        await _roleAuthRepository.ClearRolePermissionsAsync(roleId);
        return await base.DeleteAsync(roleId);
    }

    protected override object? GetEntityId(SysRoleModel entity)
    {
        return entity.SrId;
    }

    protected override void SetEntityId(SysRoleModel entity, long id)
    {
        entity.SrId = id;
    }

    protected override IEnumerable<SysRoleModel> ApplyQuery(IEnumerable<SysRoleModel> query, QueryRequest req)
    {
        if (!string.IsNullOrWhiteSpace(req.Keyword))
        {
            var keyword = req.Keyword.Trim();
            query = query.Where(role =>
                TextContains(role.SrName, keyword) ||
                TextContains(role.SrEnName, keyword) ||
                TextContains(role.SrRemark, keyword));
        }

        return query;
    }

    protected override IEnumerable<SysRoleModel> ApplySorting(IEnumerable<SysRoleModel> query, QueryRequest req)
    {
        return string.IsNullOrWhiteSpace(req.SortBy)
            ? query.OrderBy(role => role.SrId)
            : base.ApplySorting(query, req);
    }
}
