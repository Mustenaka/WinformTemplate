namespace WinformTemplate.Navigation;

public interface INavigationService
{
    Task<NavigationResult> NavigateAsync(string menuKey);
}
