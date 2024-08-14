namespace WinformTemplate.Common.Type;

/// <summary>
/// Attribute 用于备注枚举中文名称
/// </summary>
public class EnumDescriptionAttribute : Attribute
{
    public string Description { get; }

    public EnumDescriptionAttribute(string printerName)
    {
        Description = printerName;
    }
}

/// <summary>
/// 枚举类型扩展，用于输出枚举的 Description（中文）名称
/// </summary>
public static class EnumExtension
{
    public static string Description(this Enum value)
    {
        var type = value.GetType();
        var memInfo = type.GetMember(value.ToString());

        if (memInfo is { Length: > 0 })
        {
            var attributes = memInfo[0].GetCustomAttributes(typeof(EnumDescriptionAttribute), false);
            if (attributes is { Length: > 0 })
            {
                return ((EnumDescriptionAttribute)attributes[0]).Description;
            }
        }

        return value.ToString();
    }
}