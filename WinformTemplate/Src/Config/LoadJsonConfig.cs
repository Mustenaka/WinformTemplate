using System.Text.Json;

namespace WinformTemplate.Src.Config;

public class LoadJsonConfig : ILoadConfig
{
    public Dictionary<string, object> config;

    public LoadJsonConfig()
    {
        config = new Dictionary<string, object>();
    }

    public LoadJsonConfig(string filePath)
    {
        config = Read(filePath);
    }

    public Dictionary<string, object> Read(string filePath)
    {
        config = new Dictionary<string, object>();

        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            string jsonString = File.ReadAllText(filePath);

            config = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString) ?? throw new InvalidOperationException();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return config;
    }

    public Dictionary<string, object> ReadExplorer()
    {
        string configPath = "";

        OpenFileDialog dialog = new OpenFileDialog()
        {
            Multiselect = false,
            Title = @"Loading Config File | 加载配置文件",
            Filter = @"所有文件(*.json)|*.json",
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            configPath = dialog.FileName;
        }

        config = Read(configPath);

        return config;
    }
}