using Microsoft.Extensions.DependencyInjection;
using WinformTemplate.UI.Business.Demo;
using WinformTemplate.UI.Business.Sys.Login;
using WinformTemplate.UI.Business.Sys.Role;
using WinformTemplate.UI.Business.Template.Product;

namespace WinformTemplate.Navigation;

public static class PageRegistryDefaultPages
{
    public static void RegisterDefaultPages(this IPageRegistry pageRegistry)
    {
        ArgumentNullException.ThrowIfNull(pageRegistry);

        pageRegistry.Register("/sys/user", sp => sp.GetRequiredService<AccountManagementControl>());
        pageRegistry.Register("/sys/role", sp => sp.GetRequiredService<RolePlaceholderControl>());
        pageRegistry.Register("/template/product", sp => sp.GetRequiredService<ProductManagementControl>());
        pageRegistry.Register("/demo/note-ef", sp => sp.GetRequiredService<EfDemoNoteControl>());
        pageRegistry.Register("/demo/note-api", sp => sp.GetRequiredService<ApiDemoNoteControl>());
        pageRegistry.Register("/demo/note-local", sp => sp.GetRequiredService<LocalDemoNoteControl>());
    }
}
