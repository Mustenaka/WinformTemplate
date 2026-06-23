using AntdUI;
using WinformTemplate.Business.Demo.Model;
using WinformTemplate.Business.Demo.ViewModel;

namespace WinformTemplate.UI.Business.Demo;

public abstract class DemoNoteControlBase : UserControl
{
    private readonly DemoNoteManagementViewModel _viewModel;

    private readonly System.Windows.Forms.Panel _toolbar = new();
    private readonly System.Windows.Forms.Panel _main = new();
    private readonly System.Windows.Forms.Panel _pager = new();
    private readonly System.Windows.Forms.Label _sourceLabel = new();
    private readonly Input _keyword = new();
    private readonly Select _sort = new();
    private readonly Select _direction = new();
    private readonly Select _pageSize = new();
    private readonly AntdUI.Button _search = new();
    private readonly AntdUI.Button _reset = new();
    private readonly AntdUI.Button _add = new();
    private readonly AntdUI.Button _edit = new();
    private readonly AntdUI.Button _delete = new();
    private readonly AntdUI.Button _refresh = new();
    private readonly AntdUI.Button _previous = new();
    private readonly AntdUI.Button _next = new();
    private readonly Table _table = new();
    private readonly System.Windows.Forms.Label _pageInfo = new();
    private readonly System.Windows.Forms.Label _statusText = new();

    protected DemoNoteControlBase(DemoNoteManagementViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(245, 247, 250);
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

        _main.Dock = DockStyle.Fill;
        _main.BackColor = Color.White;
        _main.Padding = new Padding(12);

        _pager.Dock = DockStyle.Bottom;
        _pager.Height = 48;
        _pager.BackColor = Color.White;
        _pager.Padding = new Padding(12, 6, 12, 6);

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
        _sourceLabel.SetBounds(12, 12, 180, 32);
        _sourceLabel.Text = _viewModel.DataSourceLabel;
        _sourceLabel.TextAlign = ContentAlignment.MiddleLeft;
        _sourceLabel.Font = new Font(Font, FontStyle.Bold);

        _keyword.SetBounds(204, 12, 220, 32);
        _keyword.PlaceholderText = "搜索标题";
        _keyword.PrefixSvg = "SearchOutlined";
        _keyword.KeyPress += async (_, e) =>
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                await ApplyFiltersAndSearchAsync();
            }
        };

        _search.SetBounds(436, 12, 72, 32);
        _search.Text = "搜索";
        _search.Type = TTypeMini.Primary;
        _search.Click += async (_, _) => await ApplyFiltersAndSearchAsync();

        _reset.SetBounds(516, 12, 72, 32);
        _reset.Text = "重置";
        _reset.Click += async (_, _) => await ResetFiltersAsync();

        _refresh.SetBounds(596, 12, 72, 32);
        _refresh.Text = "刷新";
        _refresh.Click += async (_, _) => await RunActionAsync(_viewModel.LoadDataAsync);

        _sort.SetBounds(12, 52, 140, 32);
        _sort.Items.Add(new SelectItem("创建时间", "CreateAt"));
        _sort.Items.Add(new SelectItem("更新时间", "UpdateAt"));
        _sort.Items.Add(new SelectItem("标题", "Title"));
        _sort.Items.Add(new SelectItem("置顶", "Pinned"));
        _sort.SelectedValue = "CreateAt";
        _sort.SelectedValueChanged += async (_, _) => await ApplySortAsync();

        _direction.SetBounds(164, 52, 112, 32);
        _direction.Items.Add(new SelectItem("降序", "desc"));
        _direction.Items.Add(new SelectItem("升序", "asc"));
        _direction.SelectedValue = "desc";
        _direction.SelectedValueChanged += async (_, _) => await ApplySortAsync();

        _add.SetBounds(300, 52, 72, 32);
        _add.Text = "新增";
        _add.Type = TTypeMini.Primary;
        _add.Click += async (_, _) => await OpenEditorAsync(null);

        _edit.SetBounds(380, 52, 72, 32);
        _edit.Text = "修改";
        _edit.Click += async (_, _) => await OpenEditorAsync(_viewModel.SelectedNote);

        _delete.SetBounds(460, 52, 72, 32);
        _delete.Text = "删除";
        _delete.Type = TTypeMini.Error;
        _delete.Click += async (_, _) => await DeleteSelectedAsync();

        _toolbar.Controls.AddRange(new Control[]
        {
            _sourceLabel,
            _keyword,
            _search,
            _reset,
            _refresh,
            _sort,
            _direction,
            _add,
            _edit,
            _delete
        });
    }

    private void ConfigureTable()
    {
        _table.Dock = DockStyle.Fill;
        _table.Columns = new ColumnCollection
        {
            new Column(nameof(DemoNoteRow.Id), "ID", ColumnAlign.Center) { Width = "72" },
            new Column(nameof(DemoNoteRow.Title), "标题", ColumnAlign.Left) { Width = "220" },
            new Column(nameof(DemoNoteRow.Content), "内容", ColumnAlign.Left) { Width = "360" },
            new Column(nameof(DemoNoteRow.Pinned), "置顶", ColumnAlign.Center) { Width = "80" },
            new Column(nameof(DemoNoteRow.CreatedAt), "创建时间", ColumnAlign.Center) { Width = "160" },
            new Column(nameof(DemoNoteRow.UpdatedAt), "更新时间", ColumnAlign.Center) { Width = "160" }
        };
        _table.CellClick += (_, e) =>
        {
            if (e.Record is DemoNoteRow row)
            {
                _viewModel.SelectedNote = _viewModel.Notes.FirstOrDefault(note => note.Id == row.Id);
                UpdateActions();
            }
        };
    }

    private void ConfigurePager()
    {
        _previous.SetBounds(12, 8, 72, 32);
        _previous.Text = "上一页";
        _previous.Click += async (_, _) => await RunActionAsync(() => _viewModel.GoToPageAsync(_viewModel.CurrentPage - 1));

        _next.SetBounds(92, 8, 72, 32);
        _next.Text = "下一页";
        _next.Click += async (_, _) => await RunActionAsync(() => _viewModel.GoToPageAsync(_viewModel.CurrentPage + 1));

        _pageSize.SetBounds(176, 8, 96, 32);
        _pageSize.Items.Add(new SelectItem("10 / 页", 10));
        _pageSize.Items.Add(new SelectItem("20 / 页", 20));
        _pageSize.Items.Add(new SelectItem("50 / 页", 50));
        _pageSize.SelectedValue = 20;
        _pageSize.SelectedValueChanged += async (_, _) =>
        {
            if (int.TryParse(Convert.ToString(_pageSize.SelectedValue), out var size))
            {
                _viewModel.PageSize = size;
                await RunActionAsync(_viewModel.SearchAsync);
            }
        };

        _pageInfo.SetBounds(288, 12, 260, 24);
        _pageInfo.TextAlign = ContentAlignment.MiddleLeft;

        _statusText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _statusText.SetBounds(560, 12, 500, 24);
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
            case nameof(DemoNoteManagementViewModel.Notes):
                UpdateTable();
                break;
            case nameof(DemoNoteManagementViewModel.CurrentPage):
            case nameof(DemoNoteManagementViewModel.TotalCount):
            case nameof(DemoNoteManagementViewModel.TotalPages):
            case nameof(DemoNoteManagementViewModel.HasPreviousPage):
            case nameof(DemoNoteManagementViewModel.HasNextPage):
                UpdatePager();
                break;
            case nameof(DemoNoteManagementViewModel.StatusMessage):
                ShowStatus(_viewModel.StatusMessage);
                break;
            case nameof(DemoNoteManagementViewModel.IsBusy):
                UpdateBusyState();
                break;
        }
    }

    private async Task ApplyFiltersAndSearchAsync()
    {
        _viewModel.SearchKeyword = _keyword.Text;
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
        _sort.SelectedValue = "CreateAt";
        _direction.SelectedValue = "desc";
        await RunActionAsync(_viewModel.ResetSearchAsync);
    }

    private async Task OpenEditorAsync(DemoNote? source)
    {
        using var dialog = new DemoNoteEditDialog(CloneNote(source));
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = await _viewModel.SaveNoteAsync(dialog.Note);
        if (!result.Success)
        {
            ShowStatus(result.Message);
        }
    }

    private async Task DeleteSelectedAsync()
    {
        if (_viewModel.SelectedNote == null)
        {
            ShowStatus("请先选择便签。");
            return;
        }

        var confirm = MessageBox.Show(
            $"删除便签“{_viewModel.SelectedNote.Title}”？",
            "确认删除",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        var result = await _viewModel.DeleteNoteAsync(_viewModel.SelectedNote);
        if (!result.Success)
        {
            ShowStatus(result.Message);
        }
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
        _table.DataSource = _viewModel.Notes
            .Select(note => new DemoNoteRow(
                note.Id,
                note.Title,
                note.Content,
                note.PinnedText,
                note.CreateAt.ToString("yyyy-MM-dd HH:mm"),
                note.UpdateAt.ToString("yyyy-MM-dd HH:mm")))
            .ToArray();
    }

    private void UpdatePager()
    {
        _pageInfo.Text = $"第 {_viewModel.CurrentPage} / {_viewModel.TotalPages} 页，共 {_viewModel.TotalCount} 条";
        _previous.Enabled = _viewModel.HasPreviousPage && !_viewModel.IsBusy;
        _next.Enabled = _viewModel.HasNextPage && !_viewModel.IsBusy;
    }

    private void UpdateActions()
    {
        var hasSelection = _viewModel.SelectedNote != null;
        _edit.Enabled = hasSelection && !_viewModel.IsBusy;
        _delete.Enabled = hasSelection && !_viewModel.IsBusy;
        _add.Enabled = !_viewModel.IsBusy;
        _search.Enabled = !_viewModel.IsBusy;
        _reset.Enabled = !_viewModel.IsBusy;
        _refresh.Enabled = !_viewModel.IsBusy;
    }

    private void UpdateBusyState()
    {
        _search.Loading = _viewModel.IsBusy;
        _refresh.Loading = _viewModel.IsBusy;
        UpdatePager();
        UpdateActions();
    }

    private void ShowStatus(string? message)
    {
        _statusText.Text = message ?? string.Empty;
    }

    private static DemoNote CloneNote(DemoNote? source)
    {
        if (source == null)
        {
            return new DemoNote
            {
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };
        }

        return new DemoNote
        {
            Id = source.Id,
            Title = source.Title,
            Content = source.Content,
            Pinned = source.Pinned,
            CreateAt = source.CreateAt,
            UpdateAt = source.UpdateAt
        };
    }

    private sealed record DemoNoteRow(
        long Id,
        string Title,
        string Content,
        string Pinned,
        string CreatedAt,
        string UpdatedAt);

    private sealed class DemoNoteEditDialog : Window
    {
        private readonly Input _title = new();
        private readonly Input _content = new();
        private readonly System.Windows.Forms.CheckBox _pinned = new();

        public DemoNoteEditDialog(DemoNote note)
        {
            Note = note ?? throw new ArgumentNullException(nameof(note));

            Text = Note.Id > 0 ? "修改便签" : "新增便签";
            Size = new Size(460, 330);
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            BackColor = Color.White;

            Initialize();
        }

        public DemoNote Note { get; }

        private void Initialize()
        {
            AddLabel("标题", 24);
            _title.SetBounds(112, 20, 300, 32);
            _title.Text = Note.Title;

            AddLabel("内容", 68);
            _content.SetBounds(112, 64, 300, 120);
            _content.Multiline = true;
            _content.Text = Note.Content;

            _pinned.SetBounds(112, 196, 120, 24);
            _pinned.Text = "置顶";
            _pinned.Checked = Note.Pinned;

            var save = new AntdUI.Button
            {
                Text = "保存",
                Type = TTypeMini.Primary,
                Location = new Point(236, 236),
                Size = new Size(80, 32)
            };
            save.Click += (_, _) => SaveAndClose();

            var cancel = new AntdUI.Button
            {
                Text = "取消",
                Location = new Point(332, 236),
                Size = new Size(80, 32)
            };
            cancel.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.AddRange(new Control[]
            {
                _title,
                _content,
                _pinned,
                save,
                cancel
            });
        }

        private void AddLabel(string text, int y)
        {
            Controls.Add(new System.Windows.Forms.Label
            {
                Text = text,
                Location = new Point(32, y),
                Size = new Size(64, 24),
                TextAlign = ContentAlignment.MiddleLeft
            });
        }

        private void SaveAndClose()
        {
            if (string.IsNullOrWhiteSpace(_title.Text))
            {
                MessageBox.Show("标题不能为空。", "校验", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Note.Title = _title.Text.Trim();
            Note.Content = _content.Text.Trim();
            Note.Pinned = _pinned.Checked;
            Note.CreateAt = Note.CreateAt == default ? DateTime.Now : Note.CreateAt;
            Note.UpdateAt = DateTime.Now;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
