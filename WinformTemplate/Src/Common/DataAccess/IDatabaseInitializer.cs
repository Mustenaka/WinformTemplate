namespace WinformTemplate.Common.DataAccess;

public interface IDatabaseInitializer
{
    string ModuleKey { get; }

    DataSourceKind DataSourceKind { get; }

    Task InitializeAsync(CancellationToken cancellationToken = default);
}
