using System.Text.Json;
using WinformTemplate.Common.Config;
using WinformTemplate.Common.Patterns;
using WinformTemplate.Logger;

namespace WinformTemplate.Serialize;

/// <summary>
/// 泛型支持的Json序列化工具
/// </summary>
public class GlobalProjectConfig : SingletonBase<GlobalProjectConfig>
{
    private const string ConfigRelativePath = "Resources\\Config\\config.json";
    private const string ExampleConfigRelativePath = "Resources\\Config\\config.example.json";

    public ProjectConfig? Config;
    public string? configPath;

    public GlobalProjectConfig()
    {
        Config = new ProjectConfig();
        configPath = ResolveConfigPath(ConfigRelativePath);

        LoadJson();
    }

    public void LoadJson()
    {
        EnsureConfigFile();

        try
        {
            var json = File.ReadAllText(configPath!);
            Config = JsonSerializer.Deserialize<ProjectConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ProjectConfig();
        }
        catch (Exception ex)
        {
            Debug.Warn($"加载配置失败，使用默认配置: {ex.Message}");
            Config = new ProjectConfig();
        }

        LoadConfigFromApp();
    }

    public bool CheckConfigLoaded()
    {
        if (Config == null)
        {
            Debug.Info("Not loaded config");
            return false;
        }

        return true;
    }

    private void EnsureConfigFile()
    {
        if (File.Exists(configPath))
        {
            return;
        }

        var examplePath = ResolveExistingPath(ExampleConfigRelativePath);
        if (examplePath == null)
        {
            Debug.Warn("未找到 config.example.json，使用默认配置启动");
            return;
        }

        var directory = Path.GetDirectoryName(configPath!);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Copy(examplePath, configPath!, overwrite: false);
        Debug.Info($"已从示例配置创建本地配置: {configPath}");
    }

    private static string ResolveConfigPath(string relativePath)
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (File.Exists(basePath) || File.Exists(Path.Combine(AppContext.BaseDirectory, ExampleConfigRelativePath)))
        {
            return basePath;
        }

        return Path.Combine(Directory.GetCurrentDirectory(), relativePath);
    }

    private static string? ResolveExistingPath(string relativePath)
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, relativePath),
            Path.Combine(Directory.GetCurrentDirectory(), relativePath)
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private void LoadConfigFromApp()
    {
        Config!.AppVersion = Application.ProductVersion;
        Config.AppName = Application.ProductName;
    }
}
