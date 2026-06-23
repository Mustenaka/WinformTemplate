using Microsoft.Extensions.DependencyInjection;
using WinformTemplate.UI.Business.Sys.Login;
using WinformTemplate.UI.Business.Sys.Role;

namespace WinformTemplate.Navigation;

public static class PageRegistryDefaultPages
{
    public static void RegisterDefaultPages(this IPageRegistry pageRegistry)
    {
        ArgumentNullException.ThrowIfNull(pageRegistry);

        pageRegistry.Register("/sys/user", sp => sp.GetRequiredService<AccountManagementControl>());
        pageRegistry.Register("/sys/role", sp => sp.GetRequiredService<RolePlaceholderControl>());
    }
}
