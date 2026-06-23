using NUnit.Framework;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Business.Template.Service.Interface;
using WinformTemplate.Business.Template.ViewModel;
using WinformTemplate.FIO.Excel;

namespace WinformTemplate.Tests.Business.Template;

public sealed class ProductManagementViewModelTests
{
    private string? _tempDirectory;

    [SetUp]
    public void SetUp()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "WinformTemplate.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (!string.IsNullOrWhiteSpace(_tempDirectory) && Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Test]
    public async Task LoadDataAsync_UsesViewModelPagingSearchAndSortState()
    {
        var productService = new FakeProductService();
        var viewModel = CreateViewModel(productService);

        viewModel.PageSize = 1;
        viewModel.SearchKeyword = "Key";
        await viewModel.ApplySortAsync("Price", ascending: true);

        Assert.That(productService.LastSearch?.Keyword, Is.EqualTo("Key"));
        Assert.That(productService.LastSearch?.PageIndex, Is.EqualTo(1));
        Assert.That(productService.LastSearch?.PageSize, Is.EqualTo(1));
        Assert.That(productService.LastSearch?.OrderBy, Is.EqualTo("Price"));
        Assert.That(productService.LastSearch?.Ascending, Is.True);
        Assert.That(viewModel.TotalCount, Is.EqualTo(1));
        Assert.That(viewModel.Products.Single().Code, Is.EqualTo("EL-KEY-001"));
    }

    [Test]
    public async Task SaveProductAsync_AddsProductAndRefreshesCurrentPage()
    {
        var productService = new FakeProductService();
        var viewModel = CreateViewModel(productService);
        await viewModel.LoadDataAsync();

        var result = await viewModel.SaveProductAsync(new ProductModel
        {
            Name = "Camera",
            Code = "EL-CAM-001",
            CategoryId = 1,
            Price = 399,
            Stock = 5,
            Status = 0
        });

        Assert.That(result.Success, Is.True);
        Assert.That(productService.AddCount, Is.EqualTo(1));
        Assert.That(viewModel.TotalCount, Is.EqualTo(3));
        Assert.That(viewModel.Products.Any(product => product.Code == "EL-CAM-001"), Is.True);
    }

    [Test]
    public async Task ExportProductsAsync_WritesExcelWorkbook()
    {
        var productService = new FakeProductService();
        var viewModel = CreateViewModel(productService);
        viewModel.PageSize = 1;
        await viewModel.LoadDataAsync();

        var exportPath = Path.Combine(_tempDirectory!, "products.xlsx");
        var result = await viewModel.ExportProductsAsync(exportPath, currentPageOnly: true);

        Assert.That(result.Success, Is.True);
        Assert.That(File.Exists(exportPath), Is.True);

        var excel = new ExcelInteractive(exportPath);
        var sheet = excel.Read();
        Assert.That(sheet.GetRow(0).GetCell(0).StringCellValue, Is.EqualTo("ID"));
        Assert.That(sheet.GetRow(1).GetCell(2).StringCellValue, Is.EqualTo("EL-KEY-001"));
        Assert.That(sheet.LastRowNum, Is.EqualTo(1));
    }

    private static ProductManagementViewModel CreateViewModel(FakeProductService productService)
    {
        return new ProductManagementViewModel(productService, new FakeCategoryService());
    }

    private sealed class FakeProductService : IProductService
    {
        private readonly List<ProductModel> _products = new()
        {
            new ProductModel { Id = 1, Name = "Keyboard", Code = "EL-KEY-001", CategoryId = 1, Price = 129, Stock = 10, Status = 0, CreateAt = DateTime.Today, Category = new CategoryModel { Id = 1, Name = "Electronics" } },
            new ProductModel { Id = 2, Name = "Notebook", Code = "OF-NOTE-001", CategoryId = 2, Price = 9, Stock = 20, Status = 0, CreateAt = DateTime.Today, Category = new CategoryModel { Id = 2, Name = "Office" } }
        };

        public SearchCall? LastSearch { get; private set; }

        public int AddCount { get; private set; }

        public Task<ProductModel?> GetProductByIdAsync(long id)
        {
            return Task.FromResult(_products.FirstOrDefault(product => product.Id == id));
        }

        public Task<(List<ProductModel> Items, int TotalCount)> SearchProductsAsync(
            string? keyword = null,
            long? categoryId = null,
            int? status = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageIndex = 1,
            int pageSize = 10,
            string? orderBy = null,
            bool ascending = true)
        {
            LastSearch = new SearchCall(keyword, pageIndex, pageSize, orderBy, ascending);

            IEnumerable<ProductModel> query = _products;
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(product =>
                    product.Name?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true ||
                    product.Code?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true);
            }

            var total = query.Count();
            query = orderBy switch
            {
                "Price" => ascending ? query.OrderBy(product => product.Price) : query.OrderByDescending(product => product.Price),
                "Name" => ascending ? query.OrderBy(product => product.Name) : query.OrderByDescending(product => product.Name),
                _ => query.OrderByDescending(product => product.CreateAt)
            };

            var items = query
                .Skip((Math.Max(pageIndex, 1) - 1) * Math.Max(pageSize, 1))
                .Take(Math.Max(pageSize, 1))
                .ToList();

            return Task.FromResult((items, total));
        }

        public Task<List<ProductModel>> GetProductsForExportAsync(
            string? keyword = null,
            long? categoryId = null,
            int? status = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            return Task.FromResult(_products.ToList());
        }

        public Task<(bool Success, string Message)> AddProductAsync(ProductModel product)
        {
            AddCount++;
            product.Id = _products.Max(item => item.Id) + 1;
            product.Category = product.CategoryId == 1
                ? new CategoryModel { Id = 1, Name = "Electronics" }
                : null;
            _products.Add(product);
            return Task.FromResult((true, "added"));
        }

        public Task<(bool Success, string Message)> UpdateProductAsync(ProductModel product)
        {
            var index = _products.FindIndex(item => item.Id == product.Id);
            if (index < 0)
            {
                return Task.FromResult((false, "missing"));
            }

            _products[index] = product;
            return Task.FromResult((true, "updated"));
        }

        public Task<(bool Success, string Message)> DeleteProductAsync(long id)
        {
            return Task.FromResult((_products.RemoveAll(product => product.Id == id) > 0, "deleted"));
        }

        public Task<(bool Success, string Message, int DeletedCount)> BatchDeleteProductsAsync(IEnumerable<long> ids)
        {
            var idSet = ids.ToHashSet();
            var count = _products.RemoveAll(product => idSet.Contains(product.Id));
            return Task.FromResult((count > 0, "deleted", count));
        }

        public Task<(bool Success, string Message, int UpdatedCount)> BatchUpdateStatusAsync(IEnumerable<long> ids, int status)
        {
            var idSet = ids.ToHashSet();
            var count = 0;
            foreach (var product in _products.Where(product => idSet.Contains(product.Id)))
            {
                product.Status = status;
                count++;
            }

            return Task.FromResult((count > 0, "updated", count));
        }

        public Task<(bool Success, string Message, int UpdatedCount)> BatchUpdateCategoryAsync(IEnumerable<long> ids, long categoryId)
        {
            return Task.FromResult((true, "updated", ids.Count()));
        }

        public Task<(bool IsValid, List<string> Errors)> ValidateProductAsync(ProductModel product, bool isUpdate = false)
        {
            return Task.FromResult((true, new List<string>()));
        }
    }

    private sealed class FakeCategoryService : ICategoryService
    {
        private readonly List<CategoryModel> _categories = new()
        {
            new CategoryModel { Id = 1, Name = "Electronics", Status = 0 },
            new CategoryModel { Id = 2, Name = "Office", Status = 0 }
        };

        public Task<List<CategoryModel>> GetAllCategoriesAsync()
        {
            return Task.FromResult(_categories.ToList());
        }

        public Task<List<CategoryModel>> GetActiveCategoriesAsync()
        {
            return Task.FromResult(_categories.ToList());
        }

        public Task<List<CategoryModel>> GetCategoryTreeAsync()
        {
            return Task.FromResult(_categories.ToList());
        }

        public Task<CategoryModel?> GetCategoryByIdAsync(long id)
        {
            return Task.FromResult(_categories.FirstOrDefault(category => category.Id == id));
        }

        public Task<(bool Success, string Message)> AddCategoryAsync(CategoryModel category)
        {
            _categories.Add(category);
            return Task.FromResult((true, "added"));
        }

        public Task<(bool Success, string Message)> UpdateCategoryAsync(CategoryModel category)
        {
            return Task.FromResult((true, "updated"));
        }

        public Task<(bool Success, string Message)> DeleteCategoryAsync(long id)
        {
            return Task.FromResult((true, "deleted"));
        }
    }

    public sealed record SearchCall(string? Keyword, int PageIndex, int PageSize, string? OrderBy, bool Ascending);
}
