namespace WinformTemplate.Config;

/// <summary>
/// 加载Ini配置文件，由于Ini配置文件特殊性，不与Json、Xml、yaml等共享加载接口
/// </summary>
public class LoadINIConfig
{
    public Dictionary<string, Dictionary<string, object>> config;
    public LoadINIConfig()
    {
        config = new();
    }

    public LoadINIConfig(string filePath)
    {
        config = Read(filePath);
    }

    public Dictionary<string, Dictionary<string, object>> Read(string filePath)
    {
        Dictionary<string, Dictionary<string, object>> sections = new();
        string currentSection = "";

        foreach (string line in File.ReadAllLines(filePath))
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(";"))
            {
                // 跳过空行和注释行
                continue;
            }
            else if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                // 处理节（section）行
                currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                sections[currentSection] = new Dictionary<string, object>();
            }
            else
            {
                // 处理键值对行
                if (currentSection != null)
                {
                    int equalsIndex = trimmedLine.IndexOf('=');
                    if (equalsIndex > 0)
                    {
                        string key = trimmedLine.Substring(0, equalsIndex).Trim();
                        string value = trimmedLine.Substring(equalsIndex + 1).Trim();
                        sections[currentSection][key] = value;
                    }
                }
                else
                {
                    // 没有找到节的情况下，忽略键值对
                }
            }
        }
        return sections;
    }

    public Dictionary<string, Dictionary<string, object>> ReadExplorer()
    {
        string configPath = "";

        OpenFileDialog dialog = new OpenFileDialog()
        {
            Multiselect = false,
            Title = @"Loading Config File | 加载配置文件",
            Filter = @"所有文件(*.ini)|*.ini",
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            configPath = dialog.FileName;
        }

        config = Read(configPath);

        return config;
    }
}