namespace WinformTemplate.Common.WebApi;

/// <summary>
/// WebAPI 响应模型
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 错误代码
    /// </summary>
    public int? ErrorCode { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static ApiResponse<T> CreateSuccess(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message ?? "操作成功",
            Data = data
        };
    }

    /// <summary>
    /// 创建失败响应
    /// </summary>
    public static ApiResponse<T> CreateError(string message, int errorCode = 500)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode
        };
    }
}
