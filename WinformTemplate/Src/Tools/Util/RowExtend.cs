using NPOI.SS.UserModel;

namespace WinformTemplate.Tools.Util;

public static class RowExtend
{
    /// <summary>
    /// 通过checkIndex对应的数据是否存在，获取originIndex对应的数据
    /// </summary>
    /// <param name="row">IRow</param>
    /// <param name="originIndex">需要获取数据的条目</param>
    /// <param name="checkIndex">校验数据</param>
    /// <returns></returns>
    public static string? GetCellByAnother(this IRow row, int originIndex, int checkIndex)
    {
        // 如果校验下标都为null，则返回值一定为null（用于跳过逻辑）
        if (row.GetCell(checkIndex) == null)
        {
            return null;
        }

        var result = row.GetCell(originIndex)?.ToString();
        return result ?? "";
    }
}