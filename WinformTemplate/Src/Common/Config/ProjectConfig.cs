namespace WinformTemplate.Common.Config;

/// <summary>
/// 项目配置文件，可以独立使用，也可以扩展作为基类继承
/// </summary>
public class ProjectConfig
{
    public DataSourceConfig DataSource { get; set; } = new();

    public EfConfig Ef { get; set; } = new();

    public WebApiConfig WebApi { get; set; } = new();

    public LocalConfig Local { get; set; } = new();

    public SecurityConfig Security { get; set; } = new();

    #region FromConfig

    public string? DB
    {
        get => Ef.MySqlConnection;
        set => Ef.MySqlConnection = value ?? Ef.MySqlConnection;
    }

    public string? SQLiteDB
    {
        get => Ef.SQLitePath;
        set => Ef.SQLitePath = value ?? Ef.SQLitePath;
    }

    public string? DbType
    {
        get => Ef.DbType;
        set => Ef.DbType = value ?? Ef.DbType;
    }

    #endregion

    #region FromApplication

    public string? AppVersion { get; set; }

    public string? AppName { get; set; }

    #endregion
}

public class DataSourceConfig
{
    public string Default { get; set; } = "Ef";

    public Dictionary<string, string> Modules { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Sys"] = "Ef",
        ["Template"] = "Ef",
        ["Demo"] = "Ef"
    };
}

public class EfConfig
{
    public string DbType { get; set; } = "SQLite";

    public string SQLitePath { get; set; } = "./Resources/Database";

    public string MySqlConnection { get; set; } =
        "server=127.0.0.1;port=3306;user=root;database=base;password=__SET_ME__;";
}

public class WebApiConfig
{
    public string BaseUrl { get; set; } = "https://localhost:5001";

    public int TimeoutSeconds { get; set; } = 30;
}

public class LocalConfig
{
    public string SeedPath { get; set; } = "./Resources/MockData";
}

public class SecurityConfig
{
    public bool UpgradeLegacyPasswordHashOnLogin { get; set; } = true;
}
