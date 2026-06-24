namespace WinformTemplate.Navigation;

public sealed class NavigationResult
{
    private NavigationResult(NavigationStatus status, string menuKey, string message, UserControl page)
    {
        Status = status;
        MenuKey = menuKey;
        Message = message;
        Page = page;
    }

    public NavigationStatus Status { get; }

    public string MenuKey { get; }

    public string Message { get; }

    public UserControl Page { get; }

    public static NavigationResult Success(string menuKey, UserControl page)
    {
        return new NavigationResult(NavigationStatus.Success, menuKey, "OK", page);
    }

    public static NavigationResult Unauthorized(string menuKey)
    {
        return new NavigationResult(
            NavigationStatus.Unauthorized,
            menuKey,
            "无权限",
            NavigationPlaceholderPage.AccessDenied(menuKey));
    }

    public static NavigationResult NotImplemented(string menuKey)
    {
        return new NavigationResult(
            NavigationStatus.NotImplemented,
            menuKey,
            "功能未实现",
            NavigationPlaceholderPage.NotImplemented(menuKey));
    }
}
