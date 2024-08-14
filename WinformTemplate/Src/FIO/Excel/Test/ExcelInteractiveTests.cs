using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NUnit.Framework;
using WinformTemplate.Logger;

namespace WinformTemplate.FIO.Excel
{
    /// <summary>
    /// 单元测试测试Excel交互
    /// </summary>
    [TestFixture]
    public class ExcelInteractiveTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string? _testFilePathXls;
        private string? _testFilePathXlsx;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [SetUp]
        public void Setup()
        {
            Debug.InitLog4Net();

            // 创建临时的Excel文件用于测试
            _testFilePathXls = Path.Combine(Path.GetTempPath(), "test.xls");
            _testFilePathXlsx = Path.Combine(Path.GetTempPath(), "test.xlsx");
            CreateTestExcelFile(_testFilePathXls, new HSSFWorkbook());
            CreateTestExcelFile(_testFilePathXlsx, new XSSFWorkbook());

            Debug.Info("xls file path: " + _testFilePathXls);
            Debug.Info("xlsx file path: " + _testFilePathXlsx);
        }

        [TearDown]
        public void Teardown()
        {
            // 清理临时文件
            if (File.Exists(_testFilePathXls)) File.Delete(_testFilePathXls);
            if (File.Exists(_testFilePathXlsx)) File.Delete(_testFilePathXlsx);
        }

        private void CreateTestExcelFile(string filePath, IWorkbook workbook)
        {
            var sheet = workbook.CreateSheet("Sheet1");
            var row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("Test");
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            workbook.Write(fs);
        }

        [Test]
        public void Constructor_ShouldInitializeExcelInteractive()
        {
            var excelInteractiveXls = new ExcelInteractive(_testFilePathXls);
            Assert.IsNotNull(excelInteractiveXls);

            var excelInteractiveXlsx = new ExcelInteractive(_testFilePathXlsx);
            Assert.IsNotNull(excelInteractiveXlsx);
        }

        [Test]
        public void ResetFilePath_ShouldUpdateFilePath()
        {
            var excelInteractive = new ExcelInteractive(_testFilePathXls);
            Assert.AreEqual(_testFilePathXls, excelInteractive.GetFilePath());

            excelInteractive.ResetFilePath(_testFilePathXlsx);
            Assert.AreEqual(_testFilePathXlsx, excelInteractive.GetFilePath());
        }

        [Test]
        public void Read_ShouldReturnSheet()
        {
            var excelInteractive = new ExcelInteractive(_testFilePathXls);
            var sheet = excelInteractive.Read();
            Assert.IsNotNull(sheet);
            Assert.AreEqual("Sheet1", sheet.SheetName);

            excelInteractive.ResetFilePath(_testFilePathXlsx);
            sheet = excelInteractive.Read();
            Assert.IsNotNull(sheet);
            Assert.AreEqual("Sheet1", sheet.SheetName);
        }

        [Test]
        public void Write_ShouldCreateFile()
        {
            var saveFilePath = Path.Combine(Path.GetTempPath(), "saveTest.xlsx");
            var excelInteractive = new ExcelInteractive(_testFilePathXlsx);
            excelInteractive.Write(saveFilePath);

            Assert.IsTrue(File.Exists(saveFilePath));

            if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
        }
    }
}
