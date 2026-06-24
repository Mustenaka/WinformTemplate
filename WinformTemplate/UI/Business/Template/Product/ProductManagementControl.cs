using AntdUI;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Business.Template.ViewModel;

namespace WinformTemplate.UI.Business.Template.Product;

public sealed class ProductManagementControl : UserControl
{
    private readonly ProductManagementViewModel _viewModel;

    private readonly FlowLayoutPanel _toolbar = new();
    private readonly System.Windows.Forms.Panel _main = new();
    private readonly FlowLayoutPanel _pager = new();
    private readonly Input _keyword = new();
    private readonly Select _category = new();
    private readonly Select _status = new();
    private readonly Select _sort = new();
    private readonly Select _direction = new();
    private readonly Select _pageSize = new();
    private readonly System.Windows.Forms.CheckBox _exportCurrentPage = new();
    private readonly AntdUI.Button _search = new();
    private readonly AntdUI.Button _reset = new();
    private readonly AntdUI.Button _add = new();
    private readonly AntdUI.Button _edit = new();
    private readonly AntdUI.Button _delete = new();
    private readonly AntdUI.Button _export = new();
    private readonly AntdUI.Button _previous = new();
    private readonly AntdUI.Button _next = new();
    private readonly Table _table = new();
    private readonly System.Windows.Forms.Label _pageInfo = new();
    private readonly System.Windows.Forms.Label _statusText = new();

    public ProductManagementControl(ProductManagementViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(245, 247, 250);
        MinimumSize = new Size(880, 520);
        InitializeComponent();
        BindViewModel();

        Load += async (_, _) => await RunActionAsync(_viewModel.InitializeAsync);
    }

    private void InitializeComponent()
    {
        _toolbar.Dock = DockStyle.Top;
        _toolbar.Height = 96;
        _toolbar.BackColor = Color.White;
        _toolbar.Padding = new Padding(12);
        _toolbar.FlowDirection = FlowDirection.LeftToRight;
        _toolbar.WrapContents = true;
        _toolbar.AutoScroll = true;

        _main.Dock = DockStyle.Fill;
        _main.BackColor = Color.White;
        _main.Padding = new Padding(12);

        _pager.Dock = DockStyle.Bottom;
        _pager.Height = 48;
        _pager.BackColor = Color.White;
        _pager.Padding = new Padding(12, 6, 12, 6);
        _pager.FlowDirection = FlowDirection.LeftToRight;
        _pager.WrapContents = false;
        _pager.AutoScroll = true;

        ConfigureToolbar();
        ConfigureTable();
        ConfigurePager();

        _main.Controls.Add(_table);
        Controls.Add(_main);
        Controls.Add(_pager);
        Controls.Add(_toolbar);
    }

    private void ConfigureToolbar()
    {
        _keyword.Size = new Size(220, 32);
        _keyword.Margin = new Padding(0, 0, 12, 8);
        _keyword.PlaceholderText = "Search name or code";
        _keyword.PrefixSvg = "SearchOutlined";
        _keyword.KeyPress += async (_, e) =>
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                await ApplyFiltersAndSearchAsync();
            }
        };

        _category.Size = new Size(160, 32);
        _category.Margin = new Padding(0, 0, 12, 8);
        _category.PlaceholderText = "Category";

        _status.Size = new Size(136, 32);
        _status.Margin = new Padding(0, 0, 12, 8);
        _status.PlaceholderText = "Status";
        _status.Items.Add(new SelectItem("All status", string.Empty));
        _status.Items.Add(new SelectItem("Normal", 0));
        _status.Items.Add(new SelectItem("Disabled", 1));
        _status.Items.Add(new SelectItem("Out of stock", 2));

        _search.Size = new Size(80, 32);
        _search.Margin = new Padding(0, 0, 8, 8);
        _search.Text = "Search";
        _search.Type = TTypeMini.Primary;
        _search.Click += async (_, _) => await ApplyFiltersAndSearchAsync();

        _reset.Size = new Size(72, 32);
        _reset.Margin = new Padding(0, 0, 20, 8);
        _reset.Text = "Reset";
        _reset.Click += async (_, _) => await ResetFiltersAsync();

        _sort.Size = new Size(160, 32);
        _sort.Margin = new Padding(0, 0, 12, 8);
        _sort.Items.Add(new SelectItem("Created time", "CreateAt"));
        _sort.Items.Add(new SelectItem("Name", "Name"));
        _sort.Items.Add(new SelectItem("Code", "Code"));
        _sort.Items.Add(new SelectItem("Price", "Price"));
        _sort.Items.Add(new SelectItem("Stock", "Stock"));
        _sort.SelectedValue = "CreateAt";

        _direction.Size = new Size(120, 32);
        _direction.Margin = new Padding(0, 0, 20, 8);
        _direction.Items.Add(new SelectItem("Descending", "desc"));
        _direction.Items.Add(new SelectItem("Ascending", "asc"));
        _direction.SelectedValue = "desc";
        _direction.SelectedValueChanged += async (_, _) => await ApplySortAsync();
        _sort.SelectedValueChanged += async (_, _) => await ApplySortAsync();

        _add.Size = new Size(72, 32);
        _add.Margin = new Padding(0, 0, 8, 8);
        _add.Text = "Add";
        _add.Type = TTypeMini.Primary;
        _add.Click += async (_, _) => await OpenEditorAsync(null);

        _edit.Size = new Size(72, 32);
        _edit.Margin = new Padding(0, 0, 8, 8);
        _edit.Text = "Edit";
        _edit.Click += async (_, _) => await OpenEditorAsync(_viewModel.SelectedProduct);

        _delete.Size = new Size(80, 32);
        _delete.Margin = new Padding(0, 0, 12, 8);
        _delete.Text = "Delete";
        _delete.Type = TTypeMini.Error;
        _delete.Click += async (_, _) => await DeleteSelectedAsync();

        _exportCurrentPage.AutoSize = true;
        _exportCurrentPage.Height = 32;
        _exportCurrentPage.Margin = new Padding(0, 6, 12, 8);
        _exportCurrentPage.Text = "Current page";

        _export.Size = new Size(80, 32);
        _export.Margin = new Padding(0, 0, 0, 8);
        _export.Text = "Export";
        _export.Click += async (_, _) => await ExportAsync();

        _toolbar.Controls.AddRange(new Control[]
        {
            _keyword,
            _category,
            _status,
            _search,
            _reset,
            _sort,
            _direction,
            _add,
            _edit,
            _delete,
            _exportCurrentPage,
            _export
        });
    }

    private void ConfigureTable()
    {
        _table.Dock = DockStyle.Fill;
        _table.Columns = new ColumnCollection
        {
            new Column(nameof(ProductRow.Id), "ID", ColumnAlign.Center) { Width = "72" },
            new Column(nameof(ProductRow.Name), "Name", ColumnAlign.Left) { Width = "180" },
            new Column(nameof(ProductRow.Code), "Code", ColumnAlign.Left) { Width = "140" },
            new Column(nameof(ProductRow.Category), "Category", ColumnAlign.Left) { Width = "140" },
            new Column(nameof(ProductRow.Price), "Price", ColumnAlign.Right) { Width = "100" },
            new Column(nameof(ProductRow.Stock), "Stock", ColumnAlign.Right) { Width = "88" },
            new Column(nameof(ProductRow.Status), "Status", ColumnAlign.Center) { Width = "100" },
            new Column(nameof(ProductRow.CreatedAt), "Created", ColumnAlign.Center) { Width = "160" }
        };
        _table.CellClick += (_, e) =>
        {
            if (e.Record is ProductRow row)
            {
                _viewModel.SelectedProduct = _viewModel.Products.FirstOrDefault(product => product.Id == row.Id);
                UpdateActions();
            }
        };
    }

    private void ConfigurePager()
    {
        _previous.Size = new Size(72, 32);
        _previous.Margin = new Padding(0, 0, 8, 0);
        _previous.Text = "Prev";
        _previous.Click += async (_, _) => await RunActionAsync(() => _viewModel.GoToPageAsync(_viewModel.CurrentPage - 1));

        _next.Size = new Size(72, 32);
        _next.Margin = new Padding(0, 0, 12, 0);
        _next.Text = "Next";
        _next.Click += async (_, _) => await RunActionAsync(() => _viewModel.GoToPageAsync(_viewModel.CurrentPage + 1));

        _pageSize.Size = new Size(104, 32);
        _pageSize.Margin = new Padding(0, 0, 12, 0);
        _pageSize.Items.Add(new SelectItem("10 / page", 10));
        _pageSize.Items.Add(new SelectItem("20 / page", 20));
        _pageSize.Items.Add(new SelectItem("50 / page", 50));
        _pageSize.SelectedValue = 20;
        _pageSize.SelectedValueChanged += async (_, _) =>
        {
            if (TryInt(_pageSize.SelectedValue, out var size))
            {
                _viewModel.PageSize = size;
                await RunActionAsync(_viewModel.SearchAsync);
            }
        };

        _pageInfo.AutoSize = false;
        _pageInfo.Size = new Size(260, 32);
        _pageInfo.Margin = new Padding(0, 0, 12, 0);
        _pageInfo.TextAlign = ContentAlignment.MiddleLeft;

        _statusText.AutoSize = false;
        _statusText.Size = new Size(420, 32);
        _statusText.Margin = new Padding(0);
        _statusText.TextAlign = ContentAlignment.MiddleLeft;
        _statusText.ForeColor = Color.FromArgb(140, 140, 140);

        _pager.Controls.AddRange(new Control[]
        {
            _previous,
            _next,
            _pageSize,
            _pageInfo,
            _statusText
        });
    }

    private void BindViewModel()
    {
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (InvokeRequired)
            {
                Invoke(() => HandleViewModelChanged(e.PropertyName));
                return;
            }

            HandleViewModelChanged(e.PropertyName);
        };

        _viewModel.OperationCompleted += (_, message) =>
        {
            if (InvokeRequired)
            {
                Invoke(() => ShowStatus(message));
                return;
            }

            ShowStatus(message);
        };
    }

    private void HandleViewModelChanged(string? propertyName)
    {
        switch (propertyName)
        {
            case nameof(ProductManagementViewModel.Products):
                UpdateTable();
                break;
            case nameof(ProductManagementViewModel.Categories):
                UpdateCategories();
                break;
            case nameof(ProductManagementViewModel.CurrentPage):
            case nameof(ProductManagementViewModel.TotalCount):
            case nameof(ProductManagementViewModel.TotalPages):
            case nameof(ProductManagementViewModel.HasPreviousPage):
            case nameof(ProductManagementViewModel.HasNextPage):
                UpdatePager();
                break;
            case nameof(ProductManagementViewModel.StatusMessage):
                ShowStatus(_viewModel.StatusMessage);
                break;
            case nameof(ProductManagementViewModel.IsBusy):
                UpdateBusyState();
                break;
        }
    }

    private async Task ApplyFiltersAndSearchAsync()
    {
        _viewModel.SearchKeyword = _keyword.Text;
        _viewModel.SelectedCategoryId = TryLong(_category.SelectedValue, out var categoryId) ? categoryId : null;
        _viewModel.SelectedStatus = TryInt(_status.SelectedValue, out var status) ? status : null;
        await ApplySortAsync(searchAfterSort: true);
    }

    private async Task ApplySortAsync(bool searchAfterSort = false)
    {
        var sortBy = Convert.ToString(_sort.SelectedValue) ?? "CreateAt";
        var ascending = string.Equals(Convert.ToString(_direction.SelectedValue), "asc", StringComparison.OrdinalIgnoreCase);

        if (searchAfterSort)
        {
            _viewModel.SortBy = sortBy;
            _viewModel.SortAscending = ascending;
            await RunActionAsync(_viewModel.SearchAsync);
            return;
        }

        await RunActionAsync(() => _viewModel.ApplySortAsync(sortBy, ascending));
    }

    private async Task ResetFiltersAsync()
    {
        _keyword.Text = string.Empty;
        _category.SelectedValue = string.Empty;
        _status.SelectedValue = string.Empty;
        _sort.SelectedValue = "CreateAt";
        _direction.SelectedValue = "desc";
        await RunActionAsync(_viewModel.ResetSearchAsync);
    }

    private async Task OpenEditorAsync(ProductModel? source)
    {
        var product = CloneProduct(source);
        using var dialog = new ProductEditDialog(product, _viewModel.Categories);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = await _viewModel.SaveProductAsync(dialog.Product);
        if (!result.Success)
        {
            ShowStatus(result.Message);
        }
    }

    private async Task DeleteSelectedAsync()
    {
        if (_viewModel.SelectedProduct == null)
        {
            ShowStatus("Please select a product first.");
            return;
        }

        var confirm = MessageBox.Show(
            $"Delete product '{_viewModel.SelectedProduct.Name}'?",
            "Confirm delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        var result = await _viewModel.DeleteProductAsync(_viewModel.SelectedProduct);
        if (!result.Success)
        {
            ShowStatus(result.Message);
        }
    }

    private async Task ExportAsync()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Excel workbook (*.xlsx)|*.xlsx",
            FileName = $"products-{DateTime.Now:yyyyMMddHHmmss}.xlsx",
            AddExtension = true,
            DefaultExt = "xlsx"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = await _viewModel.ExportProductsAsync(dialog.FileName, _exportCurrentPage.Checked);
        ShowStatus(result.Message);
    }

    private async Task RunActionAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        finally
        {
            UpdateTable();
            UpdatePager();
            UpdateActions();
        }
    }

    private void UpdateTable()
    {
        _table.DataSource = _viewModel.Products
            .Select(product => new ProductRow(
                product.Id,
                product.Name ?? string.Empty,
                product.Code ?? string.Empty,
                product.CategoryName,
                product.PriceText,
                product.Stock?.ToString() ?? string.Empty,
                product.StatusText,
                product.CreateAt?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty))
            .ToArray();
    }

    private void UpdateCategories()
    {
        _category.Items.Clear();
        _category.Items.Add(new SelectItem("All categories", string.Empty));
        foreach (var category in _viewModel.Categories)
        {
            _category.Items.Add(new SelectItem(category.Name ?? $"Category {category.Id}", category.Id));
        }

        _category.SelectedValue = string.Empty;
    }

    private void UpdatePager()
    {
        _pageInfo.Text = $"Page {_viewModel.CurrentPage} / {_viewModel.TotalPages}, total {_viewModel.TotalCount}";
        _previous.Enabled = _viewModel.HasPreviousPage && !_viewModel.IsBusy;
        _next.Enabled = _viewModel.HasNextPage && !_viewModel.IsBusy;
    }

    private void UpdateActions()
    {
        var hasSelection = _viewModel.SelectedProduct != null;
        _edit.Enabled = hasSelection && !_viewModel.IsBusy;
        _delete.Enabled = hasSelection && !_viewModel.IsBusy;
        _add.Enabled = !_viewModel.IsBusy;
        _search.Enabled = !_viewModel.IsBusy;
        _reset.Enabled = !_viewModel.IsBusy;
        _export.Enabled = !_viewModel.IsBusy;
    }

    private void UpdateBusyState()
    {
        _search.Loading = _viewModel.IsBusy;
        _export.Loading = _viewModel.IsBusy;
        UpdatePager();
        UpdateActions();
    }

    private void ShowStatus(string? message)
    {
        _statusText.Text = message ?? string.Empty;
    }

    private static ProductModel CloneProduct(ProductModel? source)
    {
        if (source == null)
        {
            return new ProductModel
            {
                Status = 0,
                CreateAt = DateTime.Now
            };
        }

        return new ProductModel
        {
            Id = source.Id,
            Uuid = source.Uuid,
            Name = source.Name,
            Code = source.Code,
            CategoryId = source.CategoryId,
            Price = source.Price,
            Stock = source.Stock,
            Status = source.Status,
            Description = source.Description,
            ImageUrl = source.ImageUrl,
            Tags = source.Tags,
            CreateAt = source.CreateAt,
            UpdateAt = source.UpdateAt,
            Reserved1 = source.Reserved1,
            Reserved2 = source.Reserved2,
            Reserved3 = source.Reserved3
        };
    }

    private static bool TryLong(object? value, out long result)
    {
        return long.TryParse(Convert.ToString(value), out result);
    }

    private static bool TryInt(object? value, out int result)
    {
        return int.TryParse(Convert.ToString(value), out result);
    }

    private sealed record ProductRow(
        long Id,
        string Name,
        string Code,
        string Category,
        string Price,
        string Stock,
        string Status,
        string CreatedAt);

    private sealed class ProductEditDialog : Window
    {
        private readonly Input _name = new();
        private readonly Input _code = new();
        private readonly Select _category = new();
        private readonly Input _price = new();
        private readonly Input _stock = new();
        private readonly Select _status = new();
        private readonly Input _description = new();

        public ProductEditDialog(ProductModel product, IEnumerable<CategoryModel> categories)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));

            Text = Product.Id > 0 ? "Edit Product" : "Add Product";
            Size = new Size(460, 430);
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            BackColor = Color.White;

            Initialize(categories);
        }

        public ProductModel Product { get; }

        private void Initialize(IEnumerable<CategoryModel> categories)
        {
            var y = 20;
            AddLabel("Name", y);
            _name.SetBounds(132, y, 280, 32);
            _name.Text = Product.Name ?? string.Empty;
            y += 42;

            AddLabel("Code", y);
            _code.SetBounds(132, y, 280, 32);
            _code.Text = Product.Code ?? string.Empty;
            y += 42;

            AddLabel("Category", y);
            _category.SetBounds(132, y, 280, 32);
            _category.Items.Add(new SelectItem("None", string.Empty));
            foreach (var category in categories)
            {
                _category.Items.Add(new SelectItem(category.Name ?? $"Category {category.Id}", category.Id));
            }

            _category.SelectedValue = Product.CategoryId ?? 0;
            y += 42;

            AddLabel("Price", y);
            _price.SetBounds(132, y, 280, 32);
            _price.Text = Product.Price?.ToString("0.##") ?? string.Empty;
            y += 42;

            AddLabel("Stock", y);
            _stock.SetBounds(132, y, 280, 32);
            _stock.Text = Product.Stock?.ToString() ?? string.Empty;
            y += 42;

            AddLabel("Status", y);
            _status.SetBounds(132, y, 280, 32);
            _status.Items.Add(new SelectItem("Normal", 0));
            _status.Items.Add(new SelectItem("Disabled", 1));
            _status.Items.Add(new SelectItem("Out of stock", 2));
            _status.SelectedValue = Product.Status ?? 0;
            y += 42;

            AddLabel("Description", y);
            _description.SetBounds(132, y, 280, 72);
            _description.Multiline = true;
            _description.Text = Product.Description ?? string.Empty;
            y += 92;

            var save = new AntdUI.Button
            {
                Text = "Save",
                Type = TTypeMini.Primary,
                Location = new Point(236, y),
                Size = new Size(80, 32)
            };
            save.Click += (_, _) => SaveAndClose();

            var cancel = new AntdUI.Button
            {
                Text = "Cancel",
                Location = new Point(332, y),
                Size = new Size(80, 32)
            };
            cancel.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.AddRange(new Control[]
            {
                _name,
                _code,
                _category,
                _price,
                _stock,
                _status,
                _description,
                save,
                cancel
            });
        }

        private void AddLabel(string text, int y)
        {
            Controls.Add(new System.Windows.Forms.Label
            {
                Text = text,
                Location = new Point(32, y + 5),
                Size = new Size(88, 24),
                TextAlign = ContentAlignment.MiddleLeft
            });
        }

        private void SaveAndClose()
        {
            if (string.IsNullOrWhiteSpace(_name.Text) || string.IsNullOrWhiteSpace(_code.Text))
            {
                MessageBox.Show("Name and code are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Product.Name = _name.Text.Trim();
            Product.Code = _code.Text.Trim();
            Product.CategoryId = TryLong(_category.SelectedValue, out var categoryId) && categoryId > 0 ? categoryId : null;
            Product.Price = decimal.TryParse(_price.Text, out var price) ? price : null;
            Product.Stock = int.TryParse(_stock.Text, out var stock) ? stock : null;
            Product.Status = TryInt(_status.SelectedValue, out var status) ? status : 0;
            Product.Description = _description.Text;
            Product.UpdateAt = DateTime.Now;
            Product.CreateAt ??= DateTime.Now;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
