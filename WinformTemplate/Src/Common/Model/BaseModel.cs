namespace WinformTemplate.Common.Model;

/// <summary>
/// Model
///     数据层 基类，用于业务数据, 防止复写
/// </summary>
public abstract class BaseModel
{
    public int Id { get; set; }     // ID 信息标识符
}