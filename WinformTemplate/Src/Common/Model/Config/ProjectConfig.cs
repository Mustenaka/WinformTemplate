namespace WinformTemplate.Common.Model;

public class ProjectConfig
{
    #region FromConfig
    public string? DB { get; set; }
    #endregion

    #region FromApplication

    public string? AppVersion { get; set; }
    public string? AppName { get; set; }

    #endregion
}