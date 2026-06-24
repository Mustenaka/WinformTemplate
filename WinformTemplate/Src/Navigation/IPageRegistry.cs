namespace WinformTemplate.Navigation;

public interface IPageRegistry
{
    IReadOnlyCollection<string> MenuKeys { get; }

    void Register(string menuKey, Func<IServiceProvider, UserControl> factory);

    bool TryResolve(string menuKey, IServiceProvider sp, out UserControl page);
}
