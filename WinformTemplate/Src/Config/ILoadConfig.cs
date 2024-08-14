namespace WinformTemplate.Config;

public interface ILoadConfig
{
    /// <summary>
    /// Load Config | 加载配置文件
    /// </summary>
    /// <param name="filePath">配置文件路径</param>
    /// <returns>配置字典</returns>
    public Dictionary<string, object> Read(string filePath);
    
    /// <summary>
    /// Load Config and open by OpenFileDiag| 通过OpenFileDiag打开配置文件并加载
    /// </summary>
    /// <returns>配置字典</returns>
    public Dictionary<string, object> ReadExplorer();
}