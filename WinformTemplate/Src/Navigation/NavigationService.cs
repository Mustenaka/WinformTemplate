using WinformTemplate.Business.Sys.Service;
using WinformTemplate.Logger;

namespace WinformTemplate.Navigation;

public sealed class NavigationService : INavigationService
{
    private readonly ICurrentAccountAccessor _currentAccountAccessor;
    private readonly IPermissionService _permissionService;
    private readonly IPageRegistry _pageRegistry;
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(
        ICurrentAccountAccessor currentAccountAccessor,
        IPermissionService permissionService,
        IPageRegistry pageRegistry,
        IServiceProvider serviceProvider)
    {
        _currentAccountAccessor = currentAccountAccessor ?? throw new ArgumentNullException(nameof(currentAccountAccessor));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _pageRegistry = pageRegistry ?? throw new ArgumentNullException(nameof(pageRegistry));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<NavigationResult> NavigateAsync(string menuKey)
    {
        if (string.IsNullOrWhiteSpace(menuKey))
        {
            return NavigationResult.NotImplemented(menuKey);
        }

        var account = _currentAccountAccessor.CurrentAccount;
        if (account == null)
        {
            Debug.Warn($"Navigation denied because current account is empty. MenuKey={menuKey}");
            return NavigationResult.Unauthorized(menuKey);
        }

        var allowed = await _permissionService.HasPermissionAsync(account.SysId, menuKey);
        if (!allowed)
        {
            Debug.Warn($"Navigation denied. AccountId={account.SysId}, MenuKey={menuKey}");
            return NavigationResult.Unauthorized(menuKey);
        }

        if (!_pageRegistry.TryResolve(menuKey, _serviceProvider, out var page))
        {
            Debug.Warn($"Navigation target is not registered. MenuKey={menuKey}");
            return NavigationResult.NotImplemented(menuKey);
        }

        Debug.Info($"Navigation resolved. AccountId={account.SysId}, MenuKey={menuKey}, Page={page.GetType().Name}");
        return NavigationResult.Success(menuKey, page);
    }
}
