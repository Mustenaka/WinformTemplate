using System.Collections.ObjectModel;
using System.Windows.Input;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Business.Template.Service.Interface;
using WinformTemplate.Common.MVVM;
using WinformTemplate.Common.MVVM.Command;
using WinformTemplate.Logger;

namespace WinformTemplate.Business.Template.ViewModel;

/// <summary>
/// 产品管理 ViewModel
/// 提供产品列表的数据绑定和命令处理
/// </summary>
public class ProductManagementViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    // ==================== 数据绑定属性 ====================

    private ObservableCollection<ProductModel> _products = new();
    /// <summary>
    /// 产品列表
    /// </summary>
    public ObservableCollection<ProductModel> Products
    {
        get => _products;
        set => SetProperty(ref _products, value);
    }

    private ObservableCollection<CategoryModel> _categories = new();
    /// <summary>
    /// 分类列表
    /// </summary>
    public ObservableCollection<CategoryModel> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    private ProductModel? _selectedProduct;
    /// <summary>
    /// 当前选中的产品
    /// </summary>
    public ProductModel? SelectedProduct
    {
        get => _selectedProduct;
        set => SetProperty(ref _selectedProduct, value);
    }

    private List<long> _selectedProductIds = new();
    /// <summary>
    /// 选中的产品ID列表（用于批量操作）
    /// </summary>
    public List<long> SelectedProductIds
    {
        get => _selectedProductIds;
        set
        {
            SetProperty(ref _selectedProductIds, value);
            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(HasSelection));
        }
    }

    /// <summary>
    /// 选中数量
    /// </summary>
    public int SelectedCount => _selectedProductIds.Count;

    /// <summary>
    /// 是否有选中项
    /// </summary>
    public bool HasSelection => _selectedProductIds.Count > 0;

    // ==================== 搜索条件 ====================

    private string? _searchKeyword;
    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string? SearchKeyword
    {
        get => _searchKeyword;
        set
        {
            if (SetProperty(ref _searchKeyword, value))
            {
                OnSearchTextChanged();
            }
        }
    }

    private long? _selectedCategoryId;
    /// <summary>
    /// 选中的分类ID
    /// </summary>
    public long? SelectedCategoryId
    {
        get => _selectedCategoryId;
        set
        {
            if (SetProperty(ref _selectedCategoryId, value))
            {
                _ = LoadDataAsync();
            }
        }
    }

    private int? _selectedStatus;
    /// <summary>
    /// 选中的状态
    /// </summary>
    public int? SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (SetProperty(ref _selectedStatus, value))
            {
                _ = LoadDataAsync();
            }
        }
    }

    private decimal? _minPrice;
    /// <summary>
    /// 最低价格
    /// </summary>
    public decimal? MinPrice
    {
        get => _minPrice;
        set => SetProperty(ref _minPrice, value);
    }

    private decimal? _maxPrice;
    /// <summary>
    /// 最高价格
    /// </summary>
    public decimal? MaxPrice
    {
        get => _maxPrice;
        set => SetProperty(ref _maxPrice, value);
    }

    private DateTime? _startDate;
    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    private DateTime? _endDate;
    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

    // ==================== 分页属性 ====================

    private int _currentPage = 1;
    /// <summary>
    /// 当前页码
    /// </summary>
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (SetProperty(ref _currentPage, value))
            {
                _ = LoadDataAsync();
            }
        }
    }

    private int _pageSize = 20;
    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (SetProperty(ref _pageSize, value))
            {
                CurrentPage = 1; // 改变页大小时回到第一页
                _ = LoadDataAsync();
            }
        }
    }

    private int _totalCount;
    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount
    {
        get => _totalCount;
        set
        {
            SetProperty(ref _totalCount, value);
            OnPropertyChanged(nameof(TotalPages));
        }
    }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    // ==================== 排序属性 ====================

    private string _sortBy = "CreateAt";
    /// <summary>
    /// 排序字段
    /// </summary>
    public string SortBy
    {
        get => _sortBy;
        set
        {
            if (SetProperty(ref _sortBy, value))
            {
                _ = LoadDataAsync();
            }
        }
    }

    private bool _sortAscending = false;
    /// <summary>
    /// 是否升序
    /// </summary>
    public bool SortAscending
    {
        get => _sortAscending;
        set
        {
            if (SetProperty(ref _sortAscending, value))
            {
                _ = LoadDataAsync();
            }
        }
    }

    // ==================== 命令 ====================

    /// <summary>
    /// 加载数据命令
    /// </summary>
    public ICommand LoadDataCommand { get; }

    /// <summary>
    /// 搜索命令
    /// </summary>
    public ICommand SearchCommand { get; }

    /// <summary>
    /// 重置搜索命令
    /// </summary>
    public ICommand ResetSearchCommand { get; }

    /// <summary>
    /// 添加产品命令
    /// </summary>
    public ICommand AddProductCommand { get; }

    /// <summary>
    /// 编辑产品命令
    /// </summary>
    public RelayCommand<ProductModel> EditProductCommand { get; }

    /// <summary>
    /// 删除产品命令
    /// </summary>
    public RelayCommand<ProductModel> DeleteProductCommand { get; }

    /// <summary>
    /// 批量删除命令
    /// </summary>
    public ICommand BatchDeleteCommand { get; }

    /// <summary>
    /// 批量更新状态命令
    /// </summary>
    public RelayCommand<int> BatchUpdateStatusCommand { get; }

    /// <summary>
    /// 刷新命令
    /// </summary>
    public ICommand RefreshCommand { get; }

    // ==================== 事件 ====================

    /// <summary>
    /// 请求编辑产品事件
    /// </summary>
    public event EventHandler<ProductModel>? EditProductRequested;

    /// <summary>
    /// 操作完成事件
    /// </summary>
    public event EventHandler<string>? OperationCompleted;

    // ==================== 搜索防抖 ====================

    private CancellationTokenSource? _searchCts;
    private const int SearchDebounceMs = 500;

    // ==================== 构造函数 ====================

    public ProductManagementViewModel(
        IProductService productService,
        ICategoryService categoryService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));

        // 初始化命令
        LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
        SearchCommand = new RelayCommand(async () => await LoadDataAsync());
        ResetSearchCommand = new RelayCommand(ExecuteResetSearch);
        AddProductCommand = new RelayCommand(ExecuteAddProduct);
        EditProductCommand = new RelayCommand<ProductModel>(ExecuteEditProduct!);
        DeleteProductCommand = new RelayCommand<ProductModel>(async p => await ExecuteDeleteProductAsync(p));
        BatchDeleteCommand = new RelayCommand(async () => await ExecuteBatchDeleteAsync(), () => HasSelection);
        BatchUpdateStatusCommand = new RelayCommand<int>(async status => await ExecuteBatchUpdateStatusAsync(status!), _ => HasSelection);
        RefreshCommand = new RelayCommand(async () => await LoadDataAsync());
    }

    // ==================== 初始化 ====================

    /// <summary>
    /// 初始化 ViewModel（加载分类和数据）
    /// </summary>
    public new async Task InitializeAsync()
    {
        await ExecuteAsync(async () =>
        {
            // 加载分类列表
            await LoadCategoriesAsync();

            // 加载产品数据
            await LoadDataAsync();
        });
    }

    /// <summary>
    /// 加载分类列表
    /// </summary>
    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            Categories = new ObservableCollection<CategoryModel>(categories);

            Debug.Info($"已加载 {categories.Count} 个分类");
        }
        catch (Exception ex)
        {
            Debug.Error("加载分类列表失败", ex);
            StatusMessage = "加载分类列表失败: " + ex.Message;
        }
    }

    // ==================== 数据加载 ====================

    /// <summary>
    /// 加载产品数据
    /// </summary>
    public async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            var (items, totalCount) = await _productService.SearchProductsAsync(
                keyword: SearchKeyword,
                categoryId: SelectedCategoryId,
                status: SelectedStatus,
                minPrice: MinPrice,
                maxPrice: MaxPrice,
                startDate: StartDate,
                endDate: EndDate,
                pageIndex: CurrentPage,
                pageSize: PageSize,
                orderBy: SortBy,
                ascending: SortAscending);

            Products = new ObservableCollection<ProductModel>(items);
            TotalCount = totalCount;

            Debug.Info($"已加载 {items.Count} 个产品，总数: {totalCount}");
        });
    }

    /// <summary>
    /// 搜索文本变化时的防抖处理
    /// </summary>
    private async void OnSearchTextChanged()
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(SearchDebounceMs, _searchCts.Token);
            await LoadDataAsync();
        }
        catch (TaskCanceledException)
        {
            // 防抖取消，忽略
        }
    }

    // ==================== 命令执行 ====================

    /// <summary>
    /// 重置搜索条件
    /// </summary>
    private void ExecuteResetSearch()
    {
        SearchKeyword = null;
        SelectedCategoryId = null;
        SelectedStatus = null;
        MinPrice = null;
        MaxPrice = null;
        StartDate = null;
        EndDate = null;
        CurrentPage = 1;

        _ = LoadDataAsync();
    }

    /// <summary>
    /// 添加产品
    /// </summary>
    private void ExecuteAddProduct()
    {
        var newProduct = new ProductModel
        {
            Status = 0,
            CreateAt = DateTime.Now
        };

        EditProductRequested?.Invoke(this, newProduct);
    }

    /// <summary>
    /// 编辑产品
    /// </summary>
    private void ExecuteEditProduct(ProductModel? product)
    {
        if (product != null)
        {
            EditProductRequested?.Invoke(this, product);
        }
    }

    /// <summary>
    /// 删除产品
    /// </summary>
    private async Task ExecuteDeleteProductAsync(ProductModel? product)
    {
        if (product == null) return;

        var (success, message) = await _productService.DeleteProductAsync(product.Id);

        if (success)
        {
            await LoadDataAsync();
            OperationCompleted?.Invoke(this, "删除成功");
        }
        else
        {
            StatusMessage = message;
        }
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    private async Task ExecuteBatchDeleteAsync()
    {
        if (!HasSelection) return;

        var (success, message, deletedCount) = await _productService.BatchDeleteProductsAsync(SelectedProductIds);

        if (success)
        {
            SelectedProductIds.Clear();
            await LoadDataAsync();
            OperationCompleted?.Invoke(this, $"成功删除 {deletedCount} 个产品");
        }
        else
        {
            StatusMessage = message;
        }
    }

    /// <summary>
    /// 批量更新状态
    /// </summary>
    private async Task ExecuteBatchUpdateStatusAsync(int status)
    {
        if (!HasSelection) return;

        var (success, message, updatedCount) = await _productService.BatchUpdateStatusAsync(SelectedProductIds, status);

        if (success)
        {
            SelectedProductIds.Clear();
            await LoadDataAsync();
            OperationCompleted?.Invoke(this, $"成功更新 {updatedCount} 个产品的状态");
        }
        else
        {
            StatusMessage = message;
        }
    }
}
