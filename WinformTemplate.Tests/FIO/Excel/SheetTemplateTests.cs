using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NUnit.Framework;
using WinformTemplate.Logger;

namespace WinformTemplate.FIO.Excel;

public class SheetTemplateTests
{
    private string? _testFilePathXlsx;
    private string? _testFileOutputPathXlsx;
    private string? _tempDirectory;

    [SetUp]
    public void Setup()
    {
        Debug.InitLog4Net();

        _tempDirectory = Path.Combine(Path.GetTempPath(), "WinformTemplate.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);

        _testFilePathXlsx = Path.Combine(_tempDirectory, "template.xlsx");
        _testFileOutputPathXlsx = Path.Combine(_tempDirectory, "generated.xlsx");
        CreateTemplateWorkbook(_testFilePathXlsx);
        Debug.Info("xlsx file path: " + _testFilePathXlsx);
    }

    [TearDown]
    public void Teardown()
    {
        if (!string.IsNullOrWhiteSpace(_tempDirectory) && Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Test]
    public void GenerateMultiPageSheetTests()
    {
        ExcelPageInteractive.GenerateSheetsFromTemplate(_testFilePathXlsx!, _testFileOutputPathXlsx!, 3);

        Assert.IsTrue(File.Exists(_testFileOutputPathXlsx));
    }

    [Test]
    public void GenerateMultiPageSheetWithActionTests()
    {
        var excelInteractive = new ExcelInteractive(_testFilePathXlsx);
        var originalSheet = excelInteractive.Read();
        var excelPageInteractive = new ExcelPageInteractive(originalSheet);

        excelPageInteractive.ActionBeforeNewPageAdd += (sheet, _, _) =>
        {
            sheet.GetRow(6).GetCell(3).SetCellValue("TestValue");
            return 0;
        };
        excelPageInteractive.ActionAfterNewPageAdd += _ => false;

        var finalSheet = excelPageInteractive.GenerateSheetsFromTemplate(3);
        excelInteractive.Write(_testFileOutputPathXlsx!);

        Assert.IsNotNull(finalSheet);
        Assert.IsTrue(File.Exists(_testFileOutputPathXlsx));
    }

    private static void CreateTemplateWorkbook(string path)
    {
        IWorkbook workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("Template");

        for (var rowIndex = 0; rowIndex <= 8; rowIndex++)
        {
            var row = sheet.CreateRow(rowIndex);
            for (var cellIndex = 0; cellIndex <= 4; cellIndex++)
            {
                row.CreateCell(cellIndex).SetCellValue($"R{rowIndex}C{cellIndex}");
            }
        }

        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        workbook.Write(stream);
    }
}
