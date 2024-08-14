using System.Globalization;
using System.Reflection;
using NPOI.SS.UserModel;

namespace WinformTemplate.Tools.Util;

public static class CellExtend
{
    #region Extend

    /// <summary>
    /// 检查这个Cell是否含有数字"
    /// </summary>
    /// <param name="cell">ICell单元格</param>
    /// <returns>True - 存在; False - 不存在</returns>
    public static bool IsCellHaveNumber(this ICell cell)
    {
        var number = cell?.NumericCellValue;
        return number != null;
    }

    /// <summary>
    /// 检查这个Cell是否存在数据
    /// </summary>
    /// <param name="cell">ICell单元格</param>
    /// <returns>True - 存在; False - 不存在</returns>
    public static bool IsCellHaveValue(this ICell cell)
    {
        var value = cell?.ToString();
        return value != null;
    }

    /// <summary>
    /// 将Bool类型转换为程序化输出必要的字符串表达
    /// </summary>
    /// <param name="isRight"></param>
    /// <returns></returns>
    public static string TickRight(this bool isRight)
    {
        return isRight ? "\u221a" : "";
    }

    /// <summary>
    /// 将Double?数据转换为Excel表格所需要的字符串表达
    /// </summary>
    /// <param name="value">数据值</param>
    /// <param name="places">保留小数点后x位，默认为2</param>
    /// <returns></returns>
    public static string DoubleToString(this double? value, int places = 2)
    {
        // 处理特殊值
        if (double.IsNaN(value!.Value))
        {
            return "NaN";
        }

        if (double.IsPositiveInfinity(value.Value))
        {
            return "Infinity";
        }

        if (double.IsNegativeInfinity(value.Value))
        {
            return "-Infinity";
        }

        var placeSym = "F" + places.ToString();
        return value?.ToString(placeSym) ?? string.Empty;   // Default F2
    }

    /// <summary>
    /// 整形数据转字符串
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string IntToString(this int value)
    {
        return value.ToString();
    }

    /// <summary>
    /// 返回字符串（起个好名字！）
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string StringToString(this string? value)
    {
        return value ?? string.Empty;
    }
    #endregion

    #region Tool

    /// <summary>
    /// 获取单元格的值并尝试将其转换为double
    /// </summary>
    /// <param name="cell">要转换的单元格</param>
    /// <returns>转换后的double值，如果转换失败，则返回null</returns>
    public static double? GetDoubleValue(this ICell? cell)
    {
        if (cell == null)
        {
            return null;
        }

        switch (cell.CellType)
        {
            case CellType.Numeric:
                return cell.NumericCellValue;
            case CellType.String:
                // 尝试将字符串值转换为double
                if (double.TryParse(cell.StringCellValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                {
                    return result;
                }
                break;
            case CellType.Formula:
                // 处理公式单元格
                if (cell.CachedFormulaResultType == CellType.Numeric)
                {
                    return cell.NumericCellValue;
                }
                else if (cell.CachedFormulaResultType == CellType.String)
                {
                    if (double.TryParse(cell.StringCellValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double formulaResult))
                    {
                        return formulaResult;
                    }
                }
                break;
            // 可以去掉对其他类型的处理
            default:
                break;
        }

        return null;
    }

    /// <summary>
    /// 检查所有IRow中选定的单元格（ICell）是否存在Number
    /// </summary>
    /// <param name="row">IRow行</param>
    /// <param name="selectedCellIndices">被选中的数据</param>
    /// <param name="minimumRequiredConditions">最小满足条件设置数量, 默认一条</param>
    /// <returns></returns>
    public static bool ContainsNumericInCells(IRow row, int[] selectedCellIndices, int minimumRequiredConditions = 1)
    {
        var cells = new ICell[selectedCellIndices.Length];

        for (var i = 0; i < selectedCellIndices.Length; i++)
        {
            cells[i] = row.GetCell(selectedCellIndices[i]);
        }

        return CellExtend.ContainsNumericInCells(minimumRequiredConditions, cells);
    }

    /// <summary>
    /// 检查所有IRow中选定的单元格（ICell）是否存在数据
    /// </summary>
    /// <param name="row">IRow行</param>
    /// <param name="selectedCellIndices">被选中的数据</param>
    /// <param name="minimumRequiredConditions">最小满足条件设置数量, 默认一条</param>
    /// <returns></returns>
    public static bool ContainsValueInCells(IRow row, int[] selectedCellIndices, int minimumRequiredConditions = 1)
    {
        var cells = new ICell[selectedCellIndices.Length];

        for (var i = 0; i < selectedCellIndices.Length; i++)
        {
            cells[i] = row.GetCell(selectedCellIndices[i]);
        }

        return CellExtend.ContainsValueInCells(minimumRequiredConditions, cells);
    }

    /// <summary>
    /// 检查所有ICells是否包含Number
    /// </summary>
    /// <param name="minimumRequiredConditions">最小满足条件设置数量</param>
    /// <param name="cells">ICell单元格</param>
    /// <returns>True - 任意一个包含; False - 全部不包含</returns>
    public static bool ContainsNumericInCells(int minimumRequiredConditions = 1, params ICell[] cells)
    {
        // 注意 cell.Numeric 可能为 null (无法取出Number)
        var satisfied = cells.Count(cell => cell is { CellType: CellType.Numeric or CellType.Formula or CellType.String });
        return satisfied >= minimumRequiredConditions;
    }

    /// <summary>
    /// 检查所有ICells是否存在数值
    /// </summary>
    /// <param name="minimumRequiredConditions">最小满足条件设置数量</param>
    /// <param name="cells">True - 任意一个包含; False - 全部不包含</param>
    /// <returns></returns>
    public static bool ContainsValueInCells(int minimumRequiredConditions = 1, params ICell[] cells)
    {
        var satisfied = cells.Count(cell => cell?.ToString() != null);
        return satisfied >= minimumRequiredConditions;
    }

    /// <summary>
    /// 复制一个Sheet到另一个Sheet（纯内容）
    /// </summary>
    /// <param name="source">原始Sheet</param>
    /// <param name="destination">目标Sheet</param>
    /// <param name="startRow">起始行，目标Sheet从什么位置开始创建</param>
    public static void CopySheetContext(ISheet source, ISheet destination, int startRow = 0)
    {
        for (int i = 0; i <= source.LastRowNum; i++)
        {
            IRow sourceRow = source.GetRow(i);
            IRow destRow = destination.CreateRow(i + startRow);

            if (sourceRow != null)
            {
                CopyRow(sourceRow, destRow);
            }
        }
    }

    /// <summary>
    /// 复制一个Sheet到另一个Sheet（带格式）
    /// </summary>
    /// <param name="source">原始Sheet</param>
    /// <param name="destination">目标Sheet</param>
    /// <param name="startRow">起始行，目标Sheet从什么位置开始创建</param>
    public static void CopySheet(ISheet source, ISheet destination, int startRow = 0)
    {
        for (int i = 0; i <= source.LastRowNum; i++)
        {
            IRow sourceRow = source.GetRow(i);
            IRow destRow = destination.CreateRow(i + startRow);

            if (sourceRow != null)
            {
                CopyRow(sourceRow, destRow);
            }
        }

        // 复制合并单元格
        foreach (var mergedRegion in source.MergedRegions)
        {
            var newMergedRegion = new NPOI.SS.Util.CellRangeAddress(
                mergedRegion.FirstRow + startRow,
                mergedRegion.LastRow + startRow,
                mergedRegion.FirstColumn,
                mergedRegion.LastColumn);
            //destination.AddMergedRegion(newMergedRegion);     // 带校验O(N^2)复杂度，存在多个合并单元格时会卡死
            destination.AddMergedRegionUnsafe(newMergedRegion); // 不带校验O(1)复杂度，重叠单元格时会导致Excel损坏
        }

        // 复制行高和列宽
        for (int i = 0; i <= source.LastRowNum; i++)
        {
            if (source.GetRow(i) != null)
            {
                destination.GetRow(i + startRow).Height = source.GetRow(i).Height;
            }
        }

        for (int i = 0; i < source.GetRow(0).LastCellNum; i++)
        {
            destination.SetColumnWidth(i, source.GetColumnWidth(i));
        }
    }

    public static void CopyRow(IRow sourceRow, IRow destRow)
    {
        destRow.Height = sourceRow.Height; // 复制行高

        for (int j = 0; j < sourceRow.LastCellNum; j++)
        {
            ICell sourceCell = sourceRow.GetCell(j);
            ICell destCell = destRow.CreateCell(j);

            if (sourceCell != null)
            {
                CopyCell(sourceCell, destCell);
            }
        }
    }

    public static void CopyCell(ICell sourceCell, ICell destCell)
    {
        // 将新的 CellStyle 分配给目标单元格
        //destCell.CellStyle = sourceCell.CellStyle;

        // 如果sourceCell和destCell同属于同一个workbook，则直接复制CellStyle
        if (destCell.Sheet.Workbook.GetHashCode() == sourceCell.Sheet.Workbook.GetHashCode())
        {
            destCell.CellStyle = sourceCell.CellStyle;
            //destCell.CellStyle.CloneStyleFrom(sourceCell.CellStyle);  // 别用这个，信息损失
        }
        else
        {
            destCell.SetCellType(sourceCell.CellType);
        }

        // 根据单元格类型设置值
        switch (sourceCell.CellType)
        {
            case CellType.Numeric:
                destCell.SetCellValue(sourceCell.NumericCellValue);
                break;
            case CellType.String:
                //destCell.SetCellValue(sourceCell.StringCellValue);
                var sourceRichText = sourceCell.RichStringCellValue;
                var destRichText = destCell.RichStringCellValue;

                destCell.SetCellValue(sourceCell.RichStringCellValue);

                // Copy st information
                if (sourceRichText.NumFormattingRuns >= 1)
                {
                    var sourceType = sourceRichText.GetType();
                    var sourceFieldInfo = sourceType.GetField("st", BindingFlags.NonPublic | BindingFlags.Instance);

                    var destType = destRichText.GetType();
                    var destFiledInfo = destType.GetField("st", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (sourceFieldInfo != null && destFiledInfo != null)
                    {
                        var sourceValue = sourceFieldInfo.GetValue(sourceRichText) as NPOI.OpenXmlFormats.Spreadsheet.CT_Rst;
                        var destValue = destFiledInfo.GetValue(destRichText) as NPOI.OpenXmlFormats.Spreadsheet.CT_Rst;

                        destValue = sourceValue;
                    }
                }
                break;
            case CellType.Boolean:
                destCell.SetCellValue(sourceCell.BooleanCellValue);
                break;
            case CellType.Formula:
                destCell.SetCellFormula(sourceCell.CellFormula);
                break;
            case CellType.Blank:
                destCell.SetCellValue(string.Empty);
                break;
            case CellType.Unknown:
                break;
            case CellType.Error:
                break;
            default:
                destCell.SetCellValue(sourceCell.ToString());
                break;
        }
    }

    #endregion
}