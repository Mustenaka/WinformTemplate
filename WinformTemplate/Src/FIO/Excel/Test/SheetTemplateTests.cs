using NUnit.Framework;
using WinformTemplate.Logger;

namespace WinformTemplate.FIO.Excel;

public class SheetTemplateTests
{
    private string? _testFilePathXlsx;
    private string? _testFileOutputPathXlsx;

    [SetUp]
    public void Setup()
    {
        Debug.InitLog4Net();

        // 创建临时的Excel文件用于测试
        _testFilePathXlsx = "D:\\Work\\Projects\\SKY_ExcelProject\\规范文件夹\\Resources\\Business\\SKY\\Export\\Soil-sample-layout-sheet\\土工试验项目布置单-单页.xlsx";
        _testFileOutputPathXlsx = "D:\\Work\\Projects\\SKY_ExcelProject\\规范文件夹\\Resources\\Business\\SKY\\Export\\Soil-sample-layout-sheet\\土工试验项目布置单-多页(生成).xlsx";
        Debug.Info("xlsx file path: " + _testFilePathXlsx);
    }

    [TearDown]
    public void Teardown()
    {
    }

    [Test]
    public void GenerateMultiPageSheetTests()
    {
        Assert.IsNotNull(_testFilePathXlsx);
        Assert.IsNotNull(_testFileOutputPathXlsx);

        if (_testFilePathXlsx != null && _testFileOutputPathXlsx != null)
        {
            ExcelPageInteractive.GenerateSheetsFromTemplate(_testFilePathXlsx, _testFileOutputPathXlsx, 10);
        }
    }

    [Test]
    public void GenerateMultiPageSheetWithActionTests()
    {
        Assert.IsNotNull(_testFilePathXlsx);
        Assert.IsNotNull(_testFileOutputPathXlsx);

        if (_testFilePathXlsx == null || _testFileOutputPathXlsx == null)
        {
            return;
        }

        var excelInteractive = new ExcelInteractive(_testFilePathXlsx);
        var workbook = excelInteractive.GetWorkbook();
        var originalSheet = excelInteractive.Read();

        Debug.Info(originalSheet.SheetName);
        var excelPageInteractive = new ExcelPageInteractive(originalSheet);

        excelPageInteractive.ActionBeforeNewPageAdd += (sheet, _, _) =>
        {
            sheet.GetRow(6).GetCell(3).SetCellValue("TestValue");
            Debug.Info("Before new page add");
            return 0;
        };
        excelPageInteractive.ActionAfterNewPageAdd += (sheet) =>
        {
            Debug.Info("After new page add");
            return false;
        };

        excelPageInteractive.GenerateSheetsFromTemplate(10);

        //if (excelPageInteractive.finalSheet != null) workbook.AddSheet(excelPageInteractive.finalSheet, "Final");
        excelInteractive.Write(_testFileOutputPathXlsx);
    }
}