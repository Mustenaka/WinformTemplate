using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using WinformTemplate.Logger;

namespace WinformTemplate.FIO.Excel;

/// <summary>
/// 加载交互Excel交互
/// </summary>
public class ExcelInteractive
{
    private string? _filePath;        // 文件路径
    private IWorkbook? _workbook;     // 工作簿接口
    private string? _fileExt;         // 文件扩展名

    public ExcelInteractive(string? filePath)
    {
        this._filePath = filePath;
        this._fileExt = Path.GetExtension(this._filePath);

        if (_filePath == null) return;

        using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite);
        fs.Position = 0;

        _workbook = _fileExt switch
        {
            ".xls" => new HSSFWorkbook(fs),
            ".xlsx" => new XSSFWorkbook(fs),
            _ => throw new Exception("Only .xls, .xlsx")
        };
    }

    /// <summary>
    /// 重新设置文件路径
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="Exception"></exception>
    public void ResetFilePath(string? filePath)
    {
        this._filePath = filePath;
        this._fileExt = Path.GetExtension(_filePath);

        if (_filePath == null) return;

        using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite);
        fs.Position = 0;

        _workbook = _fileExt switch
        {
            ".xls" => new HSSFWorkbook(fs),
            ".xlsx" => new XSSFWorkbook(fs),
            _ => throw new Exception("Only .xls, .xlsx")
        };
    }

    /// <summary>
    /// 获取当前文件地址
    /// </summary>
    /// <returns></returns>
    public string? GetFilePath()
    {
        return _filePath;
    }

    /// <summary>
    /// 获取当前工作空间
    /// </summary>
    /// <returns></returns>
    public IWorkbook GetWorkbook()
    {
        return _workbook!;
    }

    /// <summary>
    /// 读取目标表，指定表名，默认为第一个, 读取失败返回null
    /// </summary>
    /// <returns></returns>
    public ISheet Read(string sheetName = "")
    {
        if (_filePath == null)
        {
            return null!;
        }

        // 读取指定表，默认为第一个
        return string.IsNullOrEmpty(sheetName) ? _workbook!.GetSheetAt(0) : _workbook!.GetSheet(sheetName);
    }

    /// <summary>
    /// 向目标路径写入文件
    /// </summary>
    /// <param name="saveFilePath"></param>
    /// <returns></returns>
    public bool Write(string saveFilePath)
    {
        // 检查saveFilePath 是否已经存在文件
        if (File.Exists(saveFilePath))
        {
            Debug.Warn("原位置已存在文件，将覆盖");
            File.Delete(saveFilePath);
        }

        // 写入文件
        using var result = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write);

        if (_workbook == null)
        {
            Debug.Warn("未检测到工作区数据");
            return false;
        }

        _workbook.Write(result);
        return true;
    }
}