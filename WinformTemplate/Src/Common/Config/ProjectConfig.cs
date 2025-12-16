namespace WinformTemplate.Common.Config;

/// <summary>
/// 项目配置文件，可以独立使用，也可以扩展作为基类继承
/// </summary>
public class ProjectConfig
{
    #region FromConfig

    public string? DB { get; set; }     // MySQL 数据库连接字符串
    public string? SQLiteDB { get; set; }  // SQLite 数据库文件路径
    public string? DbType { get; set; }  // 数据库类型: "SQLite" 或 "MySQL"

    #endregion 

    #region FromApplication

    public string? AppVersion { get; set; }

    public string? AppName { get; set; }

    #endregion
}