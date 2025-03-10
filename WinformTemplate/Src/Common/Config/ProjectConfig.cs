namespace WinformTemplate.Common.Config;

/// <summary>
/// 项目配置文件，可以独立使用，也可以扩展作为基类继承
/// </summary>
public class ProjectConfig
{
    #region FromConfig  

    public string? DB { get; set; }     // 正式服务

    #endregion 

    #region FromApplication

    public string? AppVersion { get; set; }

    public string? AppName { get; set; }

    #endregion
}