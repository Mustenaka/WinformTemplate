using System.Reflection;
using WinformTemplate.Common.Config;
using WinformTemplate.Common.Patterns;
using WinformTemplate.Config;
using WinformTemplate.Logger;

namespace WinformTemplate.Serialize;

/// <summary>
/// 泛型支持的Json序列化工具
/// </summary>
public class GlobalProjectConfig : SingletonBase<GlobalProjectConfig>
{
    public ProjectConfig? Config;
    public string? configPath;

    public GlobalProjectConfig()
    {
        Config = new ProjectConfig();
        configPath = ".\\Resources\\Config\\config.json";

        LoadJson();
    }

    public void LoadJson()
    {
        ILoadConfig json = new LoadJsonConfig();
        var dic = json.Read(configPath!);
        LoadConfigToModel(dic, Config!);
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

    private void LoadConfigToModel(Dictionary<string, object> config, object model)
    {
        Type modelType = model.GetType();
        foreach (var kvp in config)
        {
            PropertyInfo? property = modelType.GetProperty(kvp.Key);
            if (property != null && property.CanWrite)
            {
                try
                {
                    object? value = kvp.Value;

                    // 检查属性类型并进行适当转换
                    if (property.PropertyType == typeof(string))
                    {
                        value = Convert.ToString(value);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        value = Convert.ToInt32(value);
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        value = Convert.ToDouble(value);
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        value = Convert.ToBoolean(value);
                    }
                    // 这里可以扩展其他类型

                    property.SetValue(model, value);
                }
                catch (Exception e)
                {
                    Console.WriteLine($@"Error setting property {kvp.Key}: {e.Message}");
                }
            }
            else
            {
                Console.WriteLine($@"Property {kvp.Key} not found or not writable on {modelType.Name}");
            }
        }
    }

    private void LoadConfigFromApp()
    {
        Config!.AppVersion = Application.ProductVersion;
        Config!.AppName = Application.ProductName;
    }
}