using AntdUI;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.ViewModel;
using WinformTemplate.Common.MVVM.Extensions;

namespace WinformTemplate.UI.Business.Sys.Login;

/// <summary>
/// 账户管理页。
/// </summary>
public partial class AccountManagementControl : UserControl
{
    private readonly AccountManagementViewModel _viewModel;
    private bool _updatingPageSize;
    private bool _updatingRole;

    private FlowLayoutPanel pnlTop = null!;
    private System.Windows.Forms.Panel pnlMain = null!;
    private SplitContainer splitMain = null!;
    private System.Windows.Forms.Panel pnlLeft = null!;
    private System.Windows.Forms.Panel pnlRight = null!;
    private FlowLayoutPanel pnlPager = null!;

    private Input txtSearch = null!;
    private AntdUI.Button btnSearch = null!;
    private AntdUI.Button btnRefresh = null!;
    private AntdUI.Button btnPrev = null!;
    private AntdUI.Button btnNext = null!;
    private Select selPageSize = null!;
    private System.Windows.Forms.Label lblPageInfo = null!;

    private Table tblAccounts = null!;

    private System.Windows.Forms.Label lblEditTitle = null!;
    private System.Windows.Forms.Label lblUsername = null!;
    private Input txtUsername = null!;
    private System.Windows.Forms.Label lblPassword = null!;
    private Input txtPassword = null!;
    private System.Windows.Forms.Label lblNickname = null!;
    private Input txtNickname = null!;
    private System.Windows.Forms.Label lblRole = null!;
    private Select selRole = null!;
    private System.Windows.Forms.Label lblStatus = null!;
    private Switch swStatus = null!;
    private System.Windows.Forms.Label lblRemark = null!;
    private Input txtRemark = null!;

    private AntdUI.Button btnAdd = null!;
    private AntdUI.Button btnUpdate = null!;
    private AntdUI.Button btnDelete = null!;
    private AntdUI.Button btnToggleFreeze = null!;
    private AntdUI.Button btnResetPassword = null!;
    private AntdUI.Button btnClear = null!;

    public AccountManagementControl(AccountManagementViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        InitializeComponent();
        InitializeDataBindings();

        _viewModel.OperationSucceeded += OnOperationSucceeded;
        _viewModel.OperationFailed += OnOperationFailed;
        _viewModel.Initialize();
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        BackColor = Color.FromArgb(240, 242, 245);
        Padding = new Padding(12);
        MinimumSize = new Size(860, 520);

        pnlTop = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.White,
            Padding = new Padding(12),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        txtSearch = new Input
        {
            Size = new Size(300, 36),
            PlaceholderText = "搜索用户名或昵称",
            PrefixSvg = "SearchOutlined",
            Margin = new Padding(0, 0, 8, 0)
        };
        txtSearch.KeyPress += (_, e) =>
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                _viewModel.SearchCommand.Execute(null);
            }
        };

        btnSearch = new AntdUI.Button
        {
            Size = new Size(88, 36),
            Text = "搜索",
            Type = TTypeMini.Primary,
            Margin = new Padding(0, 0, 8, 0)
        };
        btnSearch.Click += (_, _) => _viewModel.SearchCommand.Execute(null);

        btnRefresh = new AntdUI.Button
        {
            Size = new Size(88, 36),
            Text = "刷新",
            Type = TTypeMini.Default,
            Margin = new Padding(0)
        };
        btnRefresh.Click += (_, _) => _viewModel.LoadAccountsCommand.Execute(null);
        pnlTop.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnRefresh });

        pnlMain = new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(240, 242, 245),
            Padding = new Padding(0, 10, 0, 0)
        };

        splitMain = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            FixedPanel = FixedPanel.Panel2,
            SplitterWidth = 8,
            Panel1MinSize = 420,
            Panel2MinSize = 320
        };
        splitMain.SizeChanged += (_, _) => EnsureSplitDistance();

        pnlLeft = new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(12)
        };

        tblAccounts = new Table
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        tblAccounts.Columns = new ColumnCollection
        {
            new Column("SysId", "ID", ColumnAlign.Center) { Width = "80" },
            new Column("SysAccountName", "用户名", ColumnAlign.Left) { Width = "140" },
            new Column("SysNickname", "昵称", ColumnAlign.Left) { Width = "140" },
            new Column("RoleName", "角色", ColumnAlign.Left) { Width = "120" },
            new Column("StatusText", "状态", ColumnAlign.Center) { Width = "80" }
        };
        tblAccounts.CellClick += OnTableCellClick;

        pnlPager = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            BackColor = Color.White,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };

        btnPrev = new AntdUI.Button
        {
            Size = new Size(88, 32),
            Text = "上一页",
            Type = TTypeMini.Default,
            Margin = new Padding(0, 0, 8, 0)
        };
        btnPrev.Click += (_, _) => _viewModel.PreviousPageCommand.Execute(null);

        btnNext = new AntdUI.Button
        {
            Size = new Size(88, 32),
            Text = "下一页",
            Type = TTypeMini.Default,
            Margin = new Padding(0, 0, 12, 0)
        };
        btnNext.Click += (_, _) => _viewModel.NextPageCommand.Execute(null);

        lblPageInfo = new System.Windows.Forms.Label
        {
            AutoSize = false,
            Size = new Size(260, 32),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 12, 0)
        };

        selPageSize = new Select
        {
            Size = new Size(118, 32),
            Margin = new Padding(0)
        };
        selPageSize.Items.Add(new SelectItem("10 / 页", 10));
        selPageSize.Items.Add(new SelectItem("20 / 页", 20));
        selPageSize.Items.Add(new SelectItem("50 / 页", 50));
        selPageSize.SelectedValue = 20;
        selPageSize.SelectedValueChanged += async (_, _) =>
        {
            if (_updatingPageSize)
            {
                return;
            }

            if (TryInt(selPageSize.SelectedValue, out var size))
            {
                await _viewModel.SetPageSizeAsync(size);
            }
        };

        pnlPager.Controls.AddRange(new Control[] { btnPrev, btnNext, lblPageInfo, selPageSize });
        pnlLeft.Controls.AddRange(new Control[] { tblAccounts, pnlPager });

        pnlRight = new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(12),
            AutoScroll = true
        };
        BuildEditor();

        splitMain.Panel1.Controls.Add(pnlLeft);
        splitMain.Panel2.Controls.Add(pnlRight);
        pnlMain.Controls.Add(splitMain);

        Controls.AddRange(new Control[] { pnlMain, pnlTop });

        ResumeLayout(false);
        BeginInvoke((MethodInvoker)EnsureSplitDistance);
    }

    private void BuildEditor()
    {
        var editorLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.White,
            ColumnCount = 2,
            RowCount = 8,
            Padding = new Padding(0)
        };
        editorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86));
        editorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));

        lblEditTitle = new System.Windows.Forms.Label
        {
            Text = "账户信息",
            Dock = DockStyle.Fill,
            Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        editorLayout.Controls.Add(lblEditTitle, 0, 0);
        editorLayout.SetColumnSpan(lblEditTitle, 2);

        lblUsername = CreateFieldLabel("用户名");
        txtUsername = CreateInput();
        AddEditorRow(editorLayout, 1, lblUsername, txtUsername);

        lblPassword = CreateFieldLabel("密码");
        txtPassword = CreateInput();
        txtPassword.UseSystemPasswordChar = true;
        txtPassword.PlaceholderText = "编辑时留空则不修改";
        AddEditorRow(editorLayout, 2, lblPassword, txtPassword);

        lblNickname = CreateFieldLabel("昵称");
        txtNickname = CreateInput();
        AddEditorRow(editorLayout, 3, lblNickname, txtNickname);

        lblRole = CreateFieldLabel("角色");
        selRole = new Select
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "选择角色",
            Margin = new Padding(4, 6, 0, 6)
        };
        selRole.SelectedValueChanged += (_, _) =>
        {
            if (_updatingRole)
            {
                return;
            }

            _viewModel.EditRoleId = TryLong(selRole.SelectedValue, out var roleId) ? roleId : null;
        };
        AddEditorRow(editorLayout, 4, lblRole, selRole);

        lblStatus = CreateFieldLabel("启用状态");
        swStatus = new Switch
        {
            Checked = true,
            Margin = new Padding(4, 10, 0, 6),
            Anchor = AnchorStyles.Left
        };
        AddEditorRow(editorLayout, 5, lblStatus, swStatus);

        lblRemark = CreateFieldLabel("备注");
        txtRemark = CreateInput();
        txtRemark.Multiline = true;
        AddEditorRow(editorLayout, 6, lblRemark, txtRemark);

        var actionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = Color.White,
            Padding = new Padding(0, 6, 0, 0)
        };

        btnAdd = CreateActionButton("添加", TTypeMini.Primary);
        btnAdd.Click += (_, _) => _viewModel.AddAccountCommand.Execute(null);

        btnUpdate = CreateActionButton("更新", TTypeMini.Success);
        btnUpdate.Click += (_, _) => _viewModel.UpdateAccountCommand.Execute(null);

        btnClear = CreateActionButton("清空", TTypeMini.Default);
        btnClear.Click += (_, _) => _viewModel.ClearFormCommand.Execute(null);

        btnDelete = CreateActionButton("删除", TTypeMini.Error);
        btnDelete.Click += (_, _) =>
        {
            if (MessageBox.Show("确定要删除该账户吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _viewModel.DeleteAccountCommand.Execute(null);
            }
        };

        btnToggleFreeze = CreateActionButton("冻结/解冻", TTypeMini.Warn);
        btnToggleFreeze.Click += (_, _) => _viewModel.ToggleFreezeCommand.Execute(null);

        btnResetPassword = CreateActionButton("重置密码", TTypeMini.Warn);
        btnResetPassword.Click += (_, _) =>
        {
            if (MessageBox.Show("确定要重置密码为 123456 吗？", "确认重置", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _viewModel.ResetPasswordCommand.Execute(null);
            }
        };

        actionPanel.Controls.AddRange(new Control[]
        {
            btnAdd, btnUpdate, btnClear, btnDelete, btnToggleFreeze, btnResetPassword
        });
        editorLayout.Controls.Add(actionPanel, 0, 7);
        editorLayout.SetColumnSpan(actionPanel, 2);

        pnlRight.Controls.Add(editorLayout);
    }

    private void InitializeDataBindings()
    {
        txtSearch.BindText(_viewModel, nameof(AccountManagementViewModel.SearchKeyword));
        txtUsername.BindText(_viewModel, nameof(AccountManagementViewModel.EditUsername));
        txtPassword.BindText(_viewModel, nameof(AccountManagementViewModel.EditPassword));
        txtNickname.BindText(_viewModel, nameof(AccountManagementViewModel.EditNickname));
        txtRemark.BindText(_viewModel, nameof(AccountManagementViewModel.EditRemark));
        swStatus.BindChecked(_viewModel, nameof(AccountManagementViewModel.EditStatus));

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (InvokeRequired)
            {
                Invoke(() => HandlePropertyChanged(e.PropertyName));
            }
            else
            {
                HandlePropertyChanged(e.PropertyName);
            }
        };
    }

    private void HandlePropertyChanged(string? propertyName)
    {
        switch (propertyName)
        {
            case nameof(AccountManagementViewModel.Accounts):
                UpdateAccountsTable();
                break;
            case nameof(AccountManagementViewModel.Roles):
                UpdateRolesDropdown();
                break;
            case nameof(AccountManagementViewModel.EditRoleId):
                UpdateSelectedRole();
                break;
            case nameof(AccountManagementViewModel.CurrentPage):
            case nameof(AccountManagementViewModel.TotalCount):
            case nameof(AccountManagementViewModel.TotalPages):
            case nameof(AccountManagementViewModel.HasPreviousPage):
            case nameof(AccountManagementViewModel.HasNextPage):
            case nameof(AccountManagementViewModel.PageSize):
            case nameof(AccountManagementViewModel.IsBusy):
                UpdatePager();
                UpdateBusyState();
                break;
        }
    }

    private void UpdateAccountsTable()
    {
        var data = _viewModel.Accounts.Select(account => new
        {
            account.SysId,
            account.SysAccountName,
            account.SysNickname,
            RoleName = GetRoleName(account.SysRoleId),
            StatusText = account.SysStatus ?? false ? "启用" : "冻结"
        }).ToArray();

        tblAccounts.DataSource = data;
        UpdatePager();
    }

    private void UpdateRolesDropdown()
    {
        _updatingRole = true;
        try
        {
            selRole.Items.Clear();
            selRole.Items.Add(new SelectItem("未指定", string.Empty));
            foreach (var role in _viewModel.Roles)
            {
                selRole.Items.Add(new SelectItem(role.SrName ?? $"角色 {role.SrId}", role.SrId));
            }
        }
        finally
        {
            _updatingRole = false;
        }

        UpdateSelectedRole();
        UpdateAccountsTable();
    }

    private void UpdateSelectedRole()
    {
        _updatingRole = true;
        try
        {
            selRole.SelectedValue = _viewModel.EditRoleId.HasValue ? _viewModel.EditRoleId.Value : string.Empty;
        }
        finally
        {
            _updatingRole = false;
        }
    }

    private void UpdatePager()
    {
        lblPageInfo.Text = $"第 {_viewModel.CurrentPage}/{_viewModel.TotalPages} 页，共 {_viewModel.TotalCount} 条";
        btnPrev.Enabled = _viewModel.HasPreviousPage && !_viewModel.IsBusy;
        btnNext.Enabled = _viewModel.HasNextPage && !_viewModel.IsBusy;

        _updatingPageSize = true;
        try
        {
            selPageSize.SelectedValue = _viewModel.PageSize;
        }
        finally
        {
            _updatingPageSize = false;
        }
    }

    private void UpdateBusyState()
    {
        var enabled = !_viewModel.IsBusy;
        txtSearch.Enabled = enabled;
        btnSearch.Enabled = enabled;
        btnRefresh.Enabled = enabled;
        selPageSize.Enabled = enabled;
    }

    private string GetRoleName(long? roleId)
    {
        if (roleId == null)
        {
            return "未指定";
        }

        return _viewModel.Roles.FirstOrDefault(role => role.SrId == roleId.Value)?.SrName ?? $"角色 {roleId.Value}";
    }

    private void OnTableCellClick(object? sender, TableClickEventArgs e)
    {
        if (e.Record?.GetType().GetProperty("SysId")?.GetValue(e.Record) is long accountId)
        {
            var account = _viewModel.Accounts.FirstOrDefault(item => item.SysId == accountId);
            if (account != null)
            {
                _viewModel.SelectedAccount = account;
            }
        }
    }

    private void OnOperationSucceeded(object? sender, string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => OnOperationSucceeded(sender, message));
            return;
        }

        MessageBox.Show(message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OnOperationFailed(object? sender, string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => OnOperationFailed(sender, message));
            return;
        }

        MessageBox.Show(message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _viewModel.OperationSucceeded -= OnOperationSucceeded;
            _viewModel.OperationFailed -= OnOperationFailed;
        }

        base.Dispose(disposing);
    }

    private void EnsureSplitDistance()
    {
        if (splitMain.Width <= splitMain.Panel1MinSize + splitMain.Panel2MinSize)
        {
            return;
        }

        var desiredRightWidth = Math.Min(380, splitMain.Width - splitMain.Panel1MinSize - splitMain.SplitterWidth);
        var distance = splitMain.Width - desiredRightWidth - splitMain.SplitterWidth;
        distance = Math.Clamp(distance, splitMain.Panel1MinSize, splitMain.Width - splitMain.Panel2MinSize - splitMain.SplitterWidth);
        if (distance > 0 && splitMain.SplitterDistance != distance)
        {
            splitMain.SplitterDistance = distance;
        }
    }

    private static System.Windows.Forms.Label CreateFieldLabel(string text)
    {
        return new System.Windows.Forms.Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Margin = new Padding(0, 0, 8, 0)
        };
    }

    private static Input CreateInput()
    {
        return new Input
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(4, 6, 0, 6)
        };
    }

    private static AntdUI.Button CreateActionButton(string text, TTypeMini type)
    {
        return new AntdUI.Button
        {
            Size = new Size(96, 34),
            Text = text,
            Type = type,
            Margin = new Padding(0, 0, 8, 8)
        };
    }

    private static void AddEditorRow(TableLayoutPanel layout, int row, Control label, Control editor)
    {
        layout.Controls.Add(label, 0, row);
        layout.Controls.Add(editor, 1, row);
    }

    private static bool TryInt(object? value, out int result)
    {
        if (value is int intValue)
        {
            result = intValue;
            return true;
        }

        return int.TryParse(Convert.ToString(value), out result);
    }

    private static bool TryLong(object? value, out long result)
    {
        if (value is long longValue)
        {
            result = longValue;
            return true;
        }

        return long.TryParse(Convert.ToString(value), out result);
    }
}
