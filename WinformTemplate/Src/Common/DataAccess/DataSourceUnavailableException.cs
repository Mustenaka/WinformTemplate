namespace WinformTemplate.Common.DataAccess;

public sealed class DataSourceUnavailableException : Exception
{
    public DataSourceUnavailableException(string moduleKey, string endpoint, Exception? innerException = null)
        : base($"未连接后端: 模块 {moduleKey} 的数据源不可达, endpoint {endpoint}", innerException)
    {
        ModuleKey = moduleKey;
        Endpoint = endpoint;
    }

    public string ModuleKey { get; }

    public string Endpoint { get; }
}
