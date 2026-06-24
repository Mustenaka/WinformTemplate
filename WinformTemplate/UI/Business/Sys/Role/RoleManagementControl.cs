using AntdUI;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.ViewModel;

namespace WinformTemplate.UI.Business.Sys.Role;

public sealed class RoleManagementControl : UserControl
{
    private const int DesiredPanel1MinWidth = 460;
    private const int DesiredPanel2MinWidth = 360;
    private const int DesiredRightWidth = 460;

    private readonly RoleManagementViewModel _viewModel;
    private bool _initialized;
    private bool _updatingPermissionTree;

    private readonly FlowLayoutPanel _toolbar = new();
    private readonly System.Windows.Forms.Panel _main = new();
    private readonly SplitContainer _split = new();
    private readonly System.Windows.Forms.Panel _leftPanel = new();
    private readonly TableLayoutPanel _rightLayout = new();
    private readonly Input _keyword = new();
    private readonly AntdUI.Button _search = new();
    private readonly AntdUI.Button _reset = new();
    private readonly AntdUI.Button _refresh = new();
    private readonly AntdUI.Button _add = new();
    private readonly AntdUI.Button _edit = new();
    private readonly AntdUI.Button _delete = new();
    private readonly Table _rolesTable = new();
    private readonly System.Windows.Forms.Label _permissionTitle = new();
    private readonly System.Windows.Forms.Label _permissionHint = new();
    private readonly TreeView _menuTree = new();
    private readonly FlowLayoutPanel _permissionActions = new();
    private readonly AntdUI.Button _savePermissions = new();
    private readonly System.Windows.Forms.Label _statusText = new();

    public RoleManagementControl(RoleManagementViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(245, 247, 250);
        Padding = new Padding(12);
        MinimumSize = new Size(920, 540);

        InitializeComponent();
        BindViewModel();
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        ConfigureToolbar();
        ConfigureMain();

        Controls.Add(_main);
        Controls.Add(_toolbar);

        ResumeLayout(false);

        HandleCreated += (_, _) => EnsureSplitDistance();
        SizeChanged += (_, _) => EnsureSplitDistance();
    }

    private void ConfigureToolbar()
    {
        _toolbar.Dock = DockStyle.Top;
        _toolbar.Height = 56;
        _toolbar.BackColor = Color.White;
        _toolbar.Padding = new Padding(12);
        _toolbar.FlowDirection = FlowDirection.LeftToRight;
        _toolbar.WrapContents = false;
        _toolbar.AutoScroll = true;

        _keyword.Size = new Size(240, 32);
        _keyword.Margin = new Padding(0, 0, 8, 0);
        _keyword.PlaceholderText = "搜索角色名称/英文名";
        _keyword.PrefixSvg = "SearchOutlined";
        _keyword.KeyPress += async (_, e) =>
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                await ApplySearchAsync();
            }
        };

        _search.Size = new Size(72, 32);
        _search.Margin = new Padding(0, 0, 8, 0);
        _search.Text = "搜索";
        _search.Type = TTypeMini.Primary;
        _search.Click += async (_, _) => await ApplySearchAsync();

        _reset.Size = new Size(72, 32);
        _reset.Margin = new Padding(0, 0, 8, 0);
        _reset.Text = "重置";
        _reset.Click += async (_, _) => await ResetSearchAsync();

        _refresh.Size = new Size(72, 32);
        _refresh.Margin = new Padding(0, 0, 20, 0);
        _refresh.Text = "刷新";
        _refresh.Click += async (_, _) => await RunActionAsync(_viewModel.LoadDataAsync);

        _add.Size = new Size(72, 32);
        _add.Margin = new Padding(0, 0, 8, 0);
        _add.Text = "新增";
        _add.Type = TTypeMini.Primary;
        _add.Click += async (_, _) => await OpenEditorAsync(null);

        _edit.Size = new Size(72, 32);
        _edit.Margin = new Padding(0, 0, 8, 0);
        _edit.Text = "编辑";
        _edit.Click += async (_, _) => await OpenEditorAsync(_viewModel.SelectedRole);

        _delete.Size = new Size(72, 32);
        _delete.Margin = new Padding(0);
        _delete.Text = "删除";
        _delete.Type = TTypeMini.Error;
        _delete.Click += async (_, _) => await DeleteSelectedAsync();

        _toolbar.Controls.AddRange(new Control[]
        {
            _keyword,
            _search,
            _reset,
            _refresh,
            _add,
            _edit,
            _delete
        });
    }

    private void ConfigureMain()
    {
        _main.Dock = DockStyle.Fill;
        _main.BackColor = Color.FromArgb(245, 247, 250);
        _main.Padding = new Padding(0, 10, 0, 0);

        _split.Dock = DockStyle.Fill;
        _split.Orientation = Orientation.Vertical;
        _split.FixedPanel = FixedPanel.Panel2;
        _split.SplitterWidth = 8;
        _split.Panel1MinSize = 1;
        _split.Panel2MinSize = 1;
        _split.SizeChanged += (_, _) => EnsureSplitDistance();

        ConfigureLeftPanel();
        ConfigureRightPanel();

        _split.Panel1.Controls.Add(_leftPanel);
        _split.Panel2.Controls.Add(_rightLayout);
        _main.Controls.Add(_split);
    }

    private void ConfigureLeftPanel()
    {
        _leftPanel.Dock = DockStyle.Fill;
        _leftPanel.BackColor = Color.White;
        _leftPanel.Padding = new Padding(12);

        _rolesTable.Dock = DockStyle.Fill;
        _rolesTable.Columns = new ColumnCollection
        {
            new Column(nameof(RoleRow.Id), "ID", ColumnAlign.Center) { Width = "72" },
            new Column(nameof(RoleRow.Name), "名称", ColumnAlign.Left) { Width = "140" },
            new Column(nameof(RoleRow.EnName), "英文名", ColumnAlign.Left) { Width = "140" },
            new Column(nameof(RoleRow.Remark), "备注", ColumnAlign.Left) { Width = "220" },
            new Column(nameof(RoleRow.Status), "状态", ColumnAlign.Center) { Width = "80" }
        };
        _rolesTable.CellClick += async (_, e) =>
        {
            if (e.Record is RoleRow row)
            {
                var role = _viewModel.Roles.FirstOrDefault(item => item.SrId == row.Id);
                await RunActionAsync(() => _viewModel.SelectRoleAsync(role));
            }
        };

        _statusText.Dock = DockStyle.Bottom;
        _statusText.Height = 34;
        _statusText.TextAlign = ContentAlignment.MiddleLeft;
        _statusText.ForeColor = Color.FromArgb(140, 140, 140);

        _leftPanel.Controls.Add(_rolesTable);
        _leftPanel.Controls.Add(_statusText);
    }

    private void ConfigureRightPanel()
    {
        _rightLayout.Dock = DockStyle.Fill;
        _rightLayout.BackColor = Color.White;
        _rightLayout.Padding = new Padding(12);
        _rightLayout.ColumnCount = 1;
        _rightLayout.RowCount = 4;
        _rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        _rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        _rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        _permissionTitle.Dock = DockStyle.Fill;
        _permissionTitle.Text = "菜单权限";
        _permissionTitle.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
        _permissionTitle.TextAlign = ContentAlignment.MiddleLeft;

        _permissionHint.Dock = DockStyle.Fill;
        _permissionHint.ForeColor = Color.FromArgb(140, 140, 140);
        _permissionHint.TextAlign = ContentAlignment.MiddleLeft;

        _menuTree.Dock = DockStyle.Fill;
        _menuTree.CheckBoxes = true;
        _menuTree.BorderStyle = BorderStyle.FixedSingle;
        _menuTree.HideSelection = false;
        _menuTree.AfterCheck += OnMenuTreeAfterCheck;

        _permissionActions.Dock = DockStyle.Fill;
        _permissionActions.FlowDirection = FlowDirection.RightToLeft;
        _permissionActions.WrapContents = false;
        _permissionActions.BackColor = Color.White;
        _permissionActions.Padding = new Padding(0, 8, 0, 0);

        _savePermissions.Size = new Size(112, 32);
        _savePermissions.Text = "保存权限";
        _savePermissions.Type = TTypeMini.Primary;
        _savePermissions.Click += async (_, _) => await SavePermissionsAsync();

        _permissionActions.Controls.Add(_savePermissions);

        _rightLayout.Controls.Add(_permissionTitle, 0, 0);
        _rightLayout.Controls.Add(_permissionHint, 0, 1);
        _rightLayout.Controls.Add(_menuTree, 0, 2);
        _rightLayout.Controls.Add(_permissionActions, 0, 3);
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
            case nameof(RoleManagementViewModel.Roles):
                UpdateRolesTable();
                break;
            case nameof(RoleManagementViewModel.MenuPermissions):
                UpdatePermissionTree();
                break;
            case nameof(RoleManagementViewModel.SelectedRole):
            case nameof(RoleManagementViewModel.HasSelectedRole):
                UpdatePermissionHeader();
                UpdateActions();
                break;
            case nameof(RoleManagementViewModel.StatusMessage):
                ShowStatus(_viewModel.StatusMessage);
                break;
            case nameof(RoleManagementViewModel.IsBusy):
                UpdateBusyState();
                break;
        }
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        EnsureSplitDistance();

        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await RunActionAsync(_viewModel.InitializeAsync);
    }

    private async Task ApplySearchAsync()
    {
        _viewModel.SearchKeyword = _keyword.Text;
        await RunActionAsync(_viewModel.SearchAsync);
    }

    private async Task ResetSearchAsync()
    {
        _keyword.Text = string.Empty;
        await RunActionAsync(_viewModel.ResetSearchAsync);
    }

    private async Task OpenEditorAsync(SysRoleModel? source)
    {
        if (source == null && _viewModel.IsBusy)
        {
            return;
        }

        if (source == null)
        {
            using var addDialog = new RoleEditDialog(CloneRole(null));
            if (addDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            await SaveRoleFromDialogAsync(addDialog.Role);
            return;
        }

        using var dialog = new RoleEditDialog(CloneRole(source));
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        await SaveRoleFromDialogAsync(dialog.Role);
    }

    private async Task SaveRoleFromDialogAsync(SysRoleModel role)
    {
        var result = await _viewModel.SaveRoleAsync(role);
        ShowResult(result);
    }

    private async Task DeleteSelectedAsync()
    {
        if (_viewModel.SelectedRole == null)
        {
            ShowStatus("请先选择角色。");
            return;
        }

        var confirm = MessageBox.Show(
            $"删除角色“{_viewModel.SelectedRole.SrName}”？",
            "确认删除",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        var result = await _viewModel.DeleteSelectedRoleAsync();
        ShowResult(result);
    }

    private async Task SavePermissionsAsync()
    {
        SyncTreeToViewModel();
        var result = await _viewModel.SavePermissionsAsync();
        ShowResult(result);
    }

    private async Task RunActionAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        finally
        {
            UpdateRolesTable();
            UpdatePermissionHeader();
            UpdateActions();
        }
    }

    private void UpdateRolesTable()
    {
        _rolesTable.DataSource = _viewModel.Roles
            .Select(role => new RoleRow(
                role.SrId,
                role.SrName ?? string.Empty,
                role.SrEnName ?? string.Empty,
                role.SrRemark ?? string.Empty,
                role.SrStatus == true ? "停用" : "启用"))
            .ToArray();
    }

    private void UpdatePermissionTree()
    {
        _updatingPermissionTree = true;
        _menuTree.BeginUpdate();
        try
        {
            _menuTree.Nodes.Clear();
            var nodeById = new Dictionary<long, TreeNode>();

            foreach (var item in _viewModel.MenuPermissions)
            {
                var node = new TreeNode(FormatMenuNodeText(item))
                {
                    Tag = item.MenuId,
                    Checked = item.IsChecked
                };
                nodeById[item.MenuId] = node;

                if (item.ParentId > 0 && nodeById.TryGetValue(item.ParentId, out var parentNode))
                {
                    parentNode.Nodes.Add(node);
                }
                else
                {
                    _menuTree.Nodes.Add(node);
                }
            }

            foreach (TreeNode node in _menuTree.Nodes)
            {
                ApplyParentChecks(node);
            }

            _menuTree.ExpandAll();
        }
        finally
        {
            _menuTree.EndUpdate();
            _updatingPermissionTree = false;
        }
    }

    private void UpdatePermissionHeader()
    {
        _permissionTitle.Text = _viewModel.SelectedRole == null
            ? "菜单权限"
            : $"菜单权限 - {_viewModel.SelectedRole.SrName}";
        _permissionHint.Text = _viewModel.SelectedRole == null
            ? "请选择左侧角色后分配菜单权限。"
            : "保存后需重新登录，已打开的会话菜单不会自动刷新。";
    }

    private void UpdateActions()
    {
        var enabled = !_viewModel.IsBusy;
        var hasSelection = _viewModel.HasSelectedRole;
        _add.Enabled = enabled;
        _edit.Enabled = enabled && hasSelection;
        _delete.Enabled = enabled && hasSelection;
        _savePermissions.Enabled = enabled && hasSelection;
        _search.Enabled = enabled;
        _reset.Enabled = enabled;
        _refresh.Enabled = enabled;
    }

    private void UpdateBusyState()
    {
        var enabled = !_viewModel.IsBusy;
        _keyword.Enabled = enabled;
        _menuTree.Enabled = enabled;
        _search.Loading = _viewModel.IsBusy;
        _refresh.Loading = _viewModel.IsBusy;
        _savePermissions.Loading = _viewModel.IsBusy;
        UpdateActions();
    }

    private void ShowResult((bool Success, string Message) result)
    {
        ShowStatus(result.Message);
        if (!result.Success)
        {
            MessageBox.Show(result.Message, "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        else if (result.Message.Contains("重新登录", StringComparison.Ordinal))
        {
            MessageBox.Show(result.Message, "权限已保存", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void ShowStatus(string? message)
    {
        _statusText.Text = message ?? string.Empty;
    }

    private void OnMenuTreeAfterCheck(object? sender, TreeViewEventArgs e)
    {
        if (_updatingPermissionTree || e.Node == null)
        {
            return;
        }

        _updatingPermissionTree = true;
        try
        {
            SetChildChecks(e.Node, e.Node.Checked);
            SyncParentChecks(e.Node.Parent);
        }
        finally
        {
            _updatingPermissionTree = false;
        }

        SyncTreeToViewModel();
    }

    private void SyncTreeToViewModel()
    {
        foreach (TreeNode node in _menuTree.Nodes)
        {
            SyncNodeToViewModel(node);
        }
    }

    private void SyncNodeToViewModel(TreeNode node)
    {
        if (node.Tag is long menuId)
        {
            _viewModel.SetMenuChecked(menuId, node.Checked);
        }

        foreach (TreeNode child in node.Nodes)
        {
            SyncNodeToViewModel(child);
        }
    }

    private static void SetChildChecks(TreeNode node, bool isChecked)
    {
        foreach (TreeNode child in node.Nodes)
        {
            child.Checked = isChecked;
            SetChildChecks(child, isChecked);
        }
    }

    private static void SyncParentChecks(TreeNode? node)
    {
        while (node != null)
        {
            node.Checked = node.Nodes.Cast<TreeNode>().Any(child => child.Checked);
            node = node.Parent;
        }
    }

    private static bool ApplyParentChecks(TreeNode node)
    {
        var childChecked = false;
        foreach (TreeNode child in node.Nodes)
        {
            childChecked |= ApplyParentChecks(child);
        }

        if (childChecked)
        {
            node.Checked = true;
        }

        return node.Checked;
    }

    private static string FormatMenuNodeText(RoleMenuPermissionItem item)
    {
        var route = item.IsGroup ? "分组" : item.Url;
        var disabled = item.StatusText == "停用" ? " [停用]" : string.Empty;
        return $"{item.Name} ({route}){disabled}";
    }

    private void EnsureSplitDistance()
    {
        if (_split.IsDisposed || _split.Width <= _split.SplitterWidth + 2)
        {
            return;
        }

        var canUseDesiredMinSizes = _split.Width >= DesiredPanel1MinWidth + DesiredPanel2MinWidth + _split.SplitterWidth;
        var panel1Min = canUseDesiredMinSizes ? DesiredPanel1MinWidth : 1;
        var panel2Min = canUseDesiredMinSizes ? DesiredPanel2MinWidth : 1;
        var maxDistance = _split.Width - panel2Min - _split.SplitterWidth;
        if (maxDistance < panel1Min)
        {
            return;
        }

        var rightWidth = Math.Min(DesiredRightWidth, _split.Width - panel1Min - _split.SplitterWidth);
        rightWidth = Math.Max(panel2Min, rightWidth);

        var distance = _split.Width - rightWidth - _split.SplitterWidth;
        distance = Math.Clamp(distance, panel1Min, maxDistance);

        SetSafeMinimumSizes(1, 1);
        SetSplitterDistanceIfValid(distance);
        SetSafeMinimumSizes(panel1Min, panel2Min);
        SetSplitterDistanceIfValid(distance);
    }

    private void SetSafeMinimumSizes(int panel1Min, int panel2Min)
    {
        if (_split.Panel1MinSize != panel1Min)
        {
            _split.Panel1MinSize = panel1Min;
        }

        if (_split.Panel2MinSize != panel2Min)
        {
            _split.Panel2MinSize = panel2Min;
        }
    }

    private void SetSplitterDistanceIfValid(int distance)
    {
        var maxDistance = _split.Width - _split.Panel2MinSize - _split.SplitterWidth;
        if (distance >= _split.Panel1MinSize
            && distance <= maxDistance
            && _split.SplitterDistance != distance)
        {
            _split.SplitterDistance = distance;
        }
    }

    private static SysRoleModel CloneRole(SysRoleModel? source)
    {
        if (source == null)
        {
            return new SysRoleModel
            {
                SrStatus = false,
                SrCreateAt = DateTime.Now,
                SrUpdateAt = DateTime.Now
            };
        }

        return new SysRoleModel
        {
            SrId = source.SrId,
            SrName = source.SrName,
            SrEnName = source.SrEnName,
            SrRemark = source.SrRemark,
            SrStatus = source.SrStatus,
            SrCreateAt = source.SrCreateAt,
            SrUpdateAt = source.SrUpdateAt,
            SrReserved1 = source.SrReserved1,
            SrReserved2 = source.SrReserved2,
            SrReserved3 = source.SrReserved3
        };
    }

    private sealed record RoleRow(long Id, string Name, string EnName, string Remark, string Status);

    private sealed class RoleEditDialog : Window
    {
        private readonly Input _name = new();
        private readonly Input _enName = new();
        private readonly Input _remark = new();
        private readonly System.Windows.Forms.CheckBox _enabled = new();

        public RoleEditDialog(SysRoleModel role)
        {
            Role = role ?? throw new ArgumentNullException(nameof(role));

            Text = Role.SrId > 0 ? "编辑角色" : "新增角色";
            Size = new Size(460, 330);
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            BackColor = Color.White;

            Initialize();
        }

        public SysRoleModel Role { get; }

        private void Initialize()
        {
            var y = 24;
            AddLabel("名称", y);
            _name.SetBounds(120, y, 292, 32);
            _name.Text = Role.SrName ?? string.Empty;
            y += 44;

            AddLabel("英文名", y);
            _enName.SetBounds(120, y, 292, 32);
            _enName.Text = Role.SrEnName ?? string.Empty;
            y += 44;

            AddLabel("备注", y);
            _remark.SetBounds(120, y, 292, 76);
            _remark.Multiline = true;
            _remark.Text = Role.SrRemark ?? string.Empty;
            y += 88;

            AddLabel("状态", y);
            _enabled.SetBounds(120, y + 4, 120, 24);
            _enabled.Text = "启用";
            _enabled.Checked = Role.SrStatus != true;
            y += 48;

            var save = new AntdUI.Button
            {
                Text = "保存",
                Type = TTypeMini.Primary,
                Location = new Point(236, y),
                Size = new Size(80, 32)
            };
            save.Click += (_, _) => SaveAndClose();

            var cancel = new AntdUI.Button
            {
                Text = "取消",
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
                _enName,
                _remark,
                _enabled,
                save,
                cancel
            });
        }

        private void AddLabel(string text, int y)
        {
            Controls.Add(new System.Windows.Forms.Label
            {
                Text = text,
                Location = new Point(32, y + 4),
                Size = new Size(72, 24),
                TextAlign = ContentAlignment.MiddleLeft
            });
        }

        private void SaveAndClose()
        {
            if (string.IsNullOrWhiteSpace(_name.Text) || string.IsNullOrWhiteSpace(_enName.Text))
            {
                MessageBox.Show("名称和英文名不能为空。", "校验", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Role.SrName = _name.Text.Trim();
            Role.SrEnName = _enName.Text.Trim();
            Role.SrRemark = _remark.Text.Trim();
            Role.SrStatus = !_enabled.Checked;
            Role.SrCreateAt ??= DateTime.Now;
            Role.SrUpdateAt = DateTime.Now;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
