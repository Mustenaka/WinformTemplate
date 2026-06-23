namespace WinformTemplate.Navigation;

public sealed class PageRegistry : IPageRegistry
{
    private readonly Dictionary<string, Func<IServiceProvider, UserControl>> _factories = new(StringComparer.OrdinalIgnoreCase);

    public void Register(string menuKey, Func<IServiceProvider, UserControl> factory)
    {
        if (string.IsNullOrWhiteSpace(menuKey))
        {
            throw new ArgumentException("Menu key is required.", nameof(menuKey));
        }

        _factories[menuKey] = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public bool TryResolve(string menuKey, IServiceProvider sp, out UserControl page)
    {
        if (_factories.TryGetValue(menuKey, out var factory))
        {
            page = factory(sp);
            return true;
        }

        page = null!;
        return false;
    }

    public bool Contains(string menuKey)
    {
        return _factories.ContainsKey(menuKey);
    }
}
