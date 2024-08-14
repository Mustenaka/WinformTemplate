using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using WinformTemplate.Logger;
using WinformTemplate.Tools.Util;

namespace WinformTemplate.FIO.Excel
{
    /// <summary>
    /// 模板页面交互模块
    /// </summary>
    public class ExcelPageInteractive
    {
        private IWorkbook workbook;     // 工作簿
        private ISheet originalSheet;   // 原始表格 - 单页数据
        private ISheet? cacheSheet;     // 缓存表格
        private ISheet? finalSheet;     // 最终表格

        public string OriginalSheetName { get; private set; }    // 原始表格名称
        public Func<ISheet, int, int, int>? ActionBeforeNewPageAdd { get; set; }    // Sheet, pageIndex, elementIndex, dumpElementIndex
        public Func<ISheet, bool>? ActionAfterNewPageAdd { get; set; }

        /// <summary>
        /// 载入模板表格（用于执行数据写入）
        /// </summary>
        /// <param name="originalSheet"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ExcelPageInteractive(ISheet originalSheet)
        {
            this.originalSheet = originalSheet ?? throw new ArgumentNullException(nameof(originalSheet));
            this.workbook = originalSheet.Workbook;
            this.OriginalSheetName = originalSheet.SheetName;
        }

        /// <summary>
        /// 根据模板表格生成
        /// </summary>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public ISheet GenerateSheetsFromTemplate(int pageCount)
        {
            ValidateWorkbookAndSheet();

            cacheSheet = workbook.CreateSheet("CacheSheet");
            finalSheet = workbook.CreateSheet("FinalSheet");

            // 复制模板数据到缓存Sheet
            CellExtend.CopySheet(originalSheet, cacheSheet);

            // 生成指定数量的页面
            for (var i = 0; i < pageCount; i++)
            {
                ProcessPage(i);
            }

            return finalSheet;
        }

        private void ValidateWorkbookAndSheet()
        {
            if (workbook == null || originalSheet == null)
            {
                throw new InvalidOperationException("Workbook or original sheet is null.");
            }
        }

        private void ProcessPage(int pageIndex)
        {
            var offset = pageIndex == 0 ? 0 : 1;
            var dumpElementIndex = 0;

            // 清空缓存Sheet数据，并执行Invoke
            PrepareCacheSheet();
            var needSwitchPage = ActionBeforeNewPageAdd?.Invoke(cacheSheet!, pageIndex, dumpElementIndex);

            // 将缓存数据写入最终表格
            CopyCacheToFinal(offset);

            while (needSwitchPage != 0)
            {
                dumpElementIndex = (int)needSwitchPage!;
                PrepareCacheSheet();
                needSwitchPage = ActionBeforeNewPageAdd?.Invoke(cacheSheet!, pageIndex, dumpElementIndex);
                CopyCacheToFinal(1);
            }

            // 最终表格特殊Invoke
            ActionAfterNewPageAdd?.Invoke(finalSheet!);
        }

        private void PrepareCacheSheet()
        {
            CellExtend.CopySheetContext(originalSheet, cacheSheet!);
        }

        private void CopyCacheToFinal(int offset)
        {
            Debug.Info("finalSheet!.LastRowNum: " + finalSheet!.LastRowNum);
            CellExtend.CopySheet(cacheSheet!, finalSheet!, finalSheet!.LastRowNum + offset);
        }

        public static void GenerateSheetsFromTemplate(string inputFilePath, string outputFilePath, int pageCount)
        {
            IWorkbook workbook;
            using (FileStream fs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            {
                workbook = new XSSFWorkbook(fs);
            }

            ISheet originalSheet = workbook.GetSheetAt(0);
            string originalSheetName = workbook.GetSheetName(0);
            ISheet cacheSheet = workbook.CreateSheet("CacheSheet");

            // 复制模板数据到缓存Sheet
            CellExtend.CopySheet(originalSheet, cacheSheet);

            // 在缓存Sheet中生成指定数量的页面
            for (int i = 0; i < pageCount; i++)
            {
                int startRowIndex = cacheSheet.LastRowNum + 1;
                CellExtend.CopySheet(originalSheet, cacheSheet, startRowIndex);
            }

            // 清空原始Sheet并将缓存Sheet的数据复制回原始Sheet
            workbook.RemoveSheetAt(0);
            ISheet newSheet = workbook.CreateSheet(originalSheetName);
            CellExtend.CopySheet(cacheSheet, newSheet);

            // 删除缓存Sheet
            workbook.RemoveSheetAt(workbook.GetSheetIndex(cacheSheet));

            using (FileStream fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(fs);
            }
        }
    }
}
