using System.Collections.ObjectModel;
using System.Windows.Input;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Business.Template.Service.Interface;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Common.MVVM;
using WinformTemplate.Common.MVVM.Command;
using WinformTemplate.FIO.Excel;
using WinformTemplate.Logger;

namespace WinformTemplate.Business.Template.ViewModel;

public class ProductManagementViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    private ObservableCollection<ProductModel> _products = new();
    private ObservableCollection<CategoryModel> _categories = new();
    private ProductModel? _selectedProduct;
    private List<long> _selectedProductIds = new();
    private string? _searchKeyword;
    private long? _selectedCategoryId;
    private int? _selectedStatus;
    private decimal? _minPrice;
    private decimal? _maxPrice;
    private DateTime? _startDate;
    private DateTime? _endDate;
    private int _currentPage = 1;
    private int _pageSize = 20;
    private int _totalCount;
    private string _sortBy = "CreateAt";
    private bool _sortAscending;

    public ProductManagementViewModel(
        IProductService productService,
        ICategoryService categoryService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));

        LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
        SearchCommand = new RelayCommand(async () => await SearchAsync());
        ResetSearchCommand = new RelayCommand(async () => await ResetSearchAsync());
        AddProductCommand = new RelayCommand(RequestAddProduct);
        EditProductCommand = new RelayCommand<ProductModel?>(RequestEditProduct);
        DeleteProductCommand = new RelayCommand<ProductModel?>(async product => await DeleteProductAsync(product));
        BatchDeleteCommand = new RelayCommand(async () => await BatchDeleteAsync(), () => HasSelection);
        BatchUpdateStatusCommand = new RelayCommand<int>(async status => await BatchUpdateStatusAsync(status), _ => HasSelection);
        RefreshCommand = new RelayCommand(async () => await LoadDataAsync());
        PreviousPageCommand = new RelayCommand(async () => await GoToPageAsync(CurrentPage - 1), () => HasPreviousPage);
        NextPageCommand = new RelayCommand(async () => await GoToPageAsync(CurrentPage + 1), () => HasNextPage);
        ExportCommand = new RelayCommand(() => OperationCompleted?.Invoke(this, "请选择导出文件位置。"));
    }

    public ObservableCollection<ProductModel> Products
    {
        get => _products;
        private set => SetProperty(ref _products, value);
    }

    public ObservableCollection<CategoryModel> Categories
    {
        get => _categories;
        private set => SetProperty(ref _categories, value);
    }

    public ProductModel? SelectedProduct
    {
        get => _selectedProduct;
        set => SetProperty(ref _selectedProduct, value);
    }

    public List<long> SelectedProductIds
    {
        get => _selectedProductIds;
        set
        {
            if (SetProperty(ref _selectedProductIds, value))
            {
                OnPropertyChanged(nameof(SelectedCount));
                OnPropertyChanged(nameof(HasSelection));
            }
        }
    }

    public int SelectedCount => SelectedProductIds.Count;

    public bool HasSelection => SelectedProductIds.Count > 0;

    public string? SearchKeyword
    {
        get => _searchKeyword;
        set => SetProperty(ref _searchKeyword, value);
    }

    public long? SelectedCategoryId
    {
        get => _selectedCategoryId;
        set => SetProperty(ref _selectedCategoryId, value);
    }

    public int? SelectedStatus
    {
        get => _selectedStatus;
        set => SetProperty(ref _selectedStatus, value);
    }

    public decimal? MinPrice
    {
        get => _minPrice;
        set => SetProperty(ref _minPrice, value);
    }

    public decimal? MaxPrice
    {
        get => _maxPrice;
        set => SetProperty(ref _maxPrice, value);
    }

    public DateTime? StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    public DateTime? EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

    public int CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (SetProperty(ref _currentPage, Math.Max(value, 1)))
            {
                RaisePagingPropertiesChanged();
            }
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (SetProperty(ref _pageSize, Math.Max(value, 1)))
            {
                CurrentPage = 1;
                RaisePagingPropertiesChanged();
            }
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        private set
        {
            if (SetProperty(ref _totalCount, Math.Max(value, 0)))
            {
                RaisePagingPropertiesChanged();
            }
        }
    }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;

    public string SortBy
    {
        get => _sortBy;
        set => SetProperty(ref _sortBy, string.IsNullOrWhiteSpace(value) ? "CreateAt" : value);
    }

    public bool SortAscending
    {
        get => _sortAscending;
        set => SetProperty(ref _sortAscending, value);
    }

    public ICommand LoadDataCommand { get; }

    public ICommand SearchCommand { get; }

    public ICommand ResetSearchCommand { get; }

    public ICommand AddProductCommand { get; }

    public RelayCommand<ProductModel?> EditProductCommand { get; }

    public RelayCommand<ProductModel?> DeleteProductCommand { get; }

    public ICommand BatchDeleteCommand { get; }

    public RelayCommand<int> BatchUpdateStatusCommand { get; }

    public ICommand RefreshCommand { get; }

    public ICommand PreviousPageCommand { get; }

    public ICommand NextPageCommand { get; }

    public ICommand ExportCommand { get; }

    public event EventHandler<ProductModel>? EditProductRequested;

    public event EventHandler<string>? OperationCompleted;

    public override async Task InitializeAsync()
    {
        await ExecuteAsync(async () =>
        {
            await LoadCategoriesCoreAsync();
            await LoadDataCoreAsync();
        });
    }

    public async Task LoadDataAsync()
    {
        await ExecuteAsync(LoadDataCoreAsync);
    }

    public async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadDataAsync();
    }

    public async Task ResetSearchAsync()
    {
        SearchKeyword = null;
        SelectedCategoryId = null;
        SelectedStatus = null;
        MinPrice = null;
        MaxPrice = null;
        StartDate = null;
        EndDate = null;
        SortBy = "CreateAt";
        SortAscending = false;
        CurrentPage = 1;

        await LoadDataAsync();
    }

    public async Task GoToPageAsync(int page)
    {
        CurrentPage = Math.Clamp(page, 1, TotalPages);
        await LoadDataAsync();
    }

    public async Task ApplySortAsync(string sortBy, bool ascending)
    {
        SortBy = sortBy;
        SortAscending = ascending;
        CurrentPage = 1;
        await LoadDataAsync();
    }

    public async Task<(bool Success, string Message)> SaveProductAsync(ProductModel product)
    {
        ArgumentNullException.ThrowIfNull(product);

        try
        {
            IsBusy = true;
            var result = product.Id > 0
                ? await _productService.UpdateProductAsync(product)
                : await _productService.AddProductAsync(product);

            if (result.Success)
            {
                await LoadDataCoreAsync();
                OperationCompleted?.Invoke(this, result.Message);
            }
            else
            {
                StatusMessage = result.Message;
            }

            return result;
        }
        catch (Exception ex)
        {
            Debug.Error("保存产品失败", ex);
            var message = FormatExceptionMessage(ex);
            StatusMessage = message;
            return (false, message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<(bool Success, string Message)> DeleteProductAsync(ProductModel? product)
    {
        if (product == null)
        {
            return (false, "未选择产品。");
        }

        try
        {
            IsBusy = true;
            var result = await _productService.DeleteProductAsync(product.Id);
            if (result.Success)
            {
                await LoadDataCoreAsync();
                OperationCompleted?.Invoke(this, result.Message);
            }
            else
            {
                StatusMessage = result.Message;
            }

            return result;
        }
        catch (Exception ex)
        {
            Debug.Error("删除产品失败", ex);
            var message = FormatExceptionMessage(ex);
            StatusMessage = message;
            return (false, message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<(bool Success, string Message)> ExportProductsAsync(string filePath, bool currentPageOnly)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return (false, "导出文件路径不能为空。");
        }

        try
        {
            IsBusy = true;
            var exportItems = currentPageOnly
                ? Products.ToList()
                : await _productService.GetProductsForExportAsync(
                    SearchKeyword,
                    SelectedCategoryId,
                    SelectedStatus,
                    MinPrice,
                    MaxPrice,
                    StartDate,
                    EndDate);

            ProductExcelExporter.Export(filePath, exportItems);
            var message = $"已导出 {exportItems.Count} 条产品。";
            OperationCompleted?.Invoke(this, message);
            return (true, message);
        }
        catch (Exception ex)
        {
            Debug.Error("导出产品失败", ex);
            var message = FormatExceptionMessage(ex);
            StatusMessage = message;
            return (false, message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadCategoriesCoreAsync()
    {
        var categories = await _categoryService.GetActiveCategoriesAsync();
        Categories = new ObservableCollection<CategoryModel>(categories);
    }

    private async Task LoadDataCoreAsync()
    {
        var (items, totalCount) = await _productService.SearchProductsAsync(
            SearchKeyword,
            SelectedCategoryId,
            SelectedStatus,
            MinPrice,
            MaxPrice,
            StartDate,
            EndDate,
            CurrentPage,
            PageSize,
            SortBy,
            SortAscending);

        Products = new ObservableCollection<ProductModel>(items);
        TotalCount = totalCount;
    }

    private void RequestAddProduct()
    {
        EditProductRequested?.Invoke(this, new ProductModel
        {
            Status = 0,
            CreateAt = DateTime.Now
        });
    }

    private void RequestEditProduct(ProductModel? product)
    {
        if (product != null)
        {
            EditProductRequested?.Invoke(this, product);
        }
    }

    private async Task BatchDeleteAsync()
    {
        if (!HasSelection)
        {
            return;
        }

        var (success, message, _) = await _productService.BatchDeleteProductsAsync(SelectedProductIds);
        if (success)
        {
            SelectedProductIds = new List<long>();
            await LoadDataAsync();
            OperationCompleted?.Invoke(this, message);
        }
        else
        {
            StatusMessage = message;
        }
    }

    private async Task BatchUpdateStatusAsync(int status)
    {
        if (!HasSelection)
        {
            return;
        }

        var (success, message, _) = await _productService.BatchUpdateStatusAsync(SelectedProductIds, status);
        if (success)
        {
            SelectedProductIds = new List<long>();
            await LoadDataAsync();
            OperationCompleted?.Invoke(this, message);
        }
        else
        {
            StatusMessage = message;
        }
    }

    private void RaisePagingPropertiesChanged()
    {
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(HasPreviousPage));
        OnPropertyChanged(nameof(HasNextPage));
    }

    private static string FormatExceptionMessage(Exception ex)
    {
        return ex is DataSourceUnavailableException
            ? "未连接后端，请检查数据源配置或启动 WebApi 服务。"
            : ex.Message;
    }
}
