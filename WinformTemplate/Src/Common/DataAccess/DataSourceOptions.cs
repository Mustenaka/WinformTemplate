using WinformTemplate.Common.Config;

namespace WinformTemplate.Common.DataAccess;

public sealed class DataSourceOptions
{
    private readonly IReadOnlyDictionary<string, DataSourceKind> _modules;

    public DataSourceOptions(DataSourceKind defaultKind, IReadOnlyDictionary<string, DataSourceKind> modules)
    {
        Default = defaultKind;
        _modules = modules;
    }

    public DataSourceKind Default { get; }

    public DataSourceKind Resolve(string moduleKey)
    {
        if (string.IsNullOrWhiteSpace(moduleKey))
        {
            return Default;
        }

        return _modules.TryGetValue(moduleKey, out var kind) ? kind : Default;
    }

    public static DataSourceOptions FromConfig(ProjectConfig? config)
    {
        var dataSource = config?.DataSource ?? new DataSourceConfig();
        var defaultKind = Parse(dataSource.Default, DataSourceKind.Ef, "Default");
        var modules = new Dictionary<string, DataSourceKind>(StringComparer.OrdinalIgnoreCase);

        foreach (var (moduleKey, kindText) in dataSource.Modules)
        {
            modules[moduleKey] = Parse(kindText, defaultKind, moduleKey);
        }

        return new DataSourceOptions(defaultKind, modules);
    }

    private static DataSourceKind Parse(string? value, DataSourceKind fallback, string key)
    {
        if (Enum.TryParse<DataSourceKind>(value, ignoreCase: true, out var kind))
        {
            return kind;
        }

        throw new InvalidOperationException($"DataSource.{key} 的值无效: {value ?? "<null>"}");
    }
}
