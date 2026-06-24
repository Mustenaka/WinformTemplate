using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using WinformTemplate.Business.Template.Model;

namespace WinformTemplate.FIO.Excel;

public static class ProductExcelExporter
{
    private static readonly string[] Headers =
    {
        "ID",
        "Name",
        "Code",
        "Category",
        "Price",
        "Stock",
        "Status",
        "Created At"
    };

    public static void Export(string filePath, IEnumerable<ProductModel> products)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(products);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        IWorkbook workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("Products");
        WriteHeader(sheet);
        WriteRows(sheet, products);

        for (var i = 0; i < Headers.Length; i++)
        {
            sheet.AutoSizeColumn(i);
        }

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        workbook.Write(stream);
    }

    private static void WriteHeader(ISheet sheet)
    {
        var header = sheet.CreateRow(0);
        for (var i = 0; i < Headers.Length; i++)
        {
            header.CreateCell(i).SetCellValue(Headers[i]);
        }
    }

    private static void WriteRows(ISheet sheet, IEnumerable<ProductModel> products)
    {
        var rowIndex = 1;
        foreach (var product in products)
        {
            var row = sheet.CreateRow(rowIndex++);
            row.CreateCell(0).SetCellValue(product.Id);
            row.CreateCell(1).SetCellValue(product.Name ?? string.Empty);
            row.CreateCell(2).SetCellValue(product.Code ?? string.Empty);
            row.CreateCell(3).SetCellValue(product.CategoryName);
            row.CreateCell(4).SetCellValue(product.Price.HasValue ? Convert.ToDouble(product.Price.Value) : 0);
            row.CreateCell(5).SetCellValue(product.Stock ?? 0);
            row.CreateCell(6).SetCellValue(product.StatusText);
            row.CreateCell(7).SetCellValue(product.CreateAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty);
        }
    }
}
