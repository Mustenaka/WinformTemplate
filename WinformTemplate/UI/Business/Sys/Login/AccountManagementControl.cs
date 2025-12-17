using AntdUI;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.ViewModel;

namespace WinformTemplate.UI.Business.Sys.Login;

/// <summary>
/// 账户管理用户控件
/// </summary>
public partial class AccountManagementControl : UserControl
{
    private readonly AccountManagementViewModel _viewModel;

    // 控件
    private System.Windows.Forms.Panel pnlTop = null!;
    private System.Windows.Forms.Panel pnlMain = null!;
    private System.Windows.Forms.Panel pnlLeft = null!;
    private System.Windows.Forms.Panel pnlRight = null!;

    // 搜索区域
    private Input txtSearch = null!;
    private AntdUI.Button btnSearch = null!;
    private AntdUI.Button btnRefresh = null!;

    // 列表
    private Table tblAccounts = null!;

    // 编辑区域
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

    // 操作按钮
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

        // 订阅事件
        _viewModel.OperationSucceeded += OnOperationSucceeded;
        _viewModel.OperationFailed += OnOperationFailed;

        // 初始化加载
        _viewModel.Initialize();
    }

    private void InitializeComponent()
    {
        // 主面板
        pnlTop = new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.White
        };

        pnlMain = new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(240, 242, 245)
        };

        // 左侧列表面板
        pnlLeft = new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Left,
            Width = (int)(Width * 0.6),
            BackColor = Color.White,
            Padding = new Padding(10)
        };

        // 右侧编辑面板
        pnlRight = new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(10),
            Margin = new Padding(10, 0, 0, 0)
        };

        // 搜索区域
        txtSearch = new Input
        {
            Location = new Point(20, 15),
            Size = new Size(300, 35),
            PlaceholderText = "搜索用户名或昵称",
            PrefixSvg = "SearchOutlined"
        };
        txtSearch.KeyPress += (s, e) =>
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                _viewModel.SearchCommand.Execute(null);
            }
        };

        btnSearch = new AntdUI.Button
        {
            Location = new Point(330, 15),
            Size = new Size(80, 35),
            Text = "搜索",
            Type = TTypeMini.Primary
        };
        btnSearch.Click += (s, e) => _viewModel.SearchCommand.Execute(null);

        btnRefresh = new AntdUI.Button
        {
            Location = new Point(420, 15),
            Size = new Size(80, 35),
            Text = "刷新",
            Type = TTypeMini.Default
        };
        btnRefresh.Click += (s, e) => _viewModel.LoadAccountsCommand.Execute(null);

        // 账户列表表格
        tblAccounts = new Table
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 10, 0, 0)
        };
        tblAccounts.Columns = new ColumnCollection
        {
            new Column("SysId", "ID", ColumnAlign.Center) { Width = "80" },
            new Column("SysAccountName", "用户名", ColumnAlign.Left) { Width = "120" },
            new Column("SysNickname", "昵称", ColumnAlign.Left) { Width = "120" },
            new Column("RoleName", "角色", ColumnAlign.Left) { Width = "100" },
            new Column("StatusText", "状态", ColumnAlign.Center) { Width = "80" }
        };
        tblAccounts.CellClick += OnTableCellClick;

        // 编辑区域标题
        lblEditTitle = new System.Windows.Forms.Label
        {
            Text = "账户信息",
            Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold),
            Location = new Point(10, 10),
            Size = new Size(200, 30)
        };

        int yPos = 50;
        int labelWidth = 80;
        int inputWidth = 240;
        int rowHeight = 50;

        // 用户名
        lblUsername = new System.Windows.Forms.Label
        {
            Text = "用户名",
            Location = new Point(10, yPos + 10),
            Size = new Size(labelWidth, 25)
        };
        txtUsername = new Input
        {
            Location = new Point(100, yPos),
            Size = new Size(inputWidth, 35)
        };
        txtUsername.TextChanged += (s, e) => _viewModel.EditUsername = txtUsername.Text;
        yPos += rowHeight;

        // 密码
        lblPassword = new System.Windows.Forms.Label
        {
            Text = "密码",
            Location = new Point(10, yPos + 10),
            Size = new Size(labelWidth, 25)
        };
        txtPassword = new Input
        {
            Location = new Point(100, yPos),
            Size = new Size(inputWidth, 35),
            UseSystemPasswordChar = true,
            PlaceholderText = "编辑时留空则不修改"
        };
        txtPassword.TextChanged += (s, e) => _viewModel.EditPassword = txtPassword.Text;
        yPos += rowHeight;

        // 昵称
        lblNickname = new System.Windows.Forms.Label
        {
            Text = "昵称",
            Location = new Point(10, yPos + 10),
            Size = new Size(labelWidth, 25)
        };
        txtNickname = new Input
        {
            Location = new Point(100, yPos),
            Size = new Size(inputWidth, 35)
        };
        txtNickname.TextChanged += (s, e) => _viewModel.EditNickname = txtNickname.Text;
        yPos += rowHeight;

        // 角色
        lblRole = new System.Windows.Forms.Label
        {
            Text = "角色",
            Location = new Point(10, yPos + 10),
            Size = new Size(labelWidth, 25)
        };
        selRole = new Select
        {
            Location = new Point(100, yPos),
            Size = new Size(inputWidth, 35),
            PlaceholderText = "选择角色"
        };
        selRole.SelectedValueChanged += (s, e) =>
        {
            if (selRole.SelectedValue is long roleId)
            {
                _viewModel.EditRoleId = roleId;
            }
            else if (selRole.SelectedValue != null && long.TryParse(selRole.SelectedValue.ToString(), out var parsedId))
            {
                _viewModel.EditRoleId = parsedId;
            }
        };
        yPos += rowHeight;

        // 状态
        lblStatus = new System.Windows.Forms.Label
        {
            Text = "启用状态",
            Location = new Point(10, yPos + 10),
            Size = new Size(labelWidth, 25)
        };
        swStatus = new Switch
        {
            Location = new Point(100, yPos + 5),
            Checked = true
        };
        swStatus.CheckedChanged += (s, e) => _viewModel.EditStatus = swStatus.Checked;
        yPos += rowHeight;

        // 备注
        lblRemark = new System.Windows.Forms.Label
        {
            Text = "备注",
            Location = new Point(10, yPos + 10),
            Size = new Size(labelWidth, 25)
        };
        txtRemark = new Input
        {
            Location = new Point(100, yPos),
            Size = new Size(inputWidth, 70),
            Multiline = true
        };
        txtRemark.TextChanged += (s, e) => _viewModel.EditRemark = txtRemark.Text;
        yPos += 90;

        // 操作按钮
        int btnY = yPos;
        btnAdd = new AntdUI.Button
        {
            Location = new Point(10, btnY),
            Size = new Size(100, 35),
            Text = "添加",
            Type = TTypeMini.Primary
        };
        btnAdd.Click += (s, e) => _viewModel.AddAccountCommand.Execute(null);

        btnUpdate = new AntdUI.Button
        {
            Location = new Point(120, btnY),
            Size = new Size(100, 35),
            Text = "更新",
            Type = TTypeMini.Success
        };
        btnUpdate.Click += (s, e) => _viewModel.UpdateAccountCommand.Execute(null);

        btnClear = new AntdUI.Button
        {
            Location = new Point(230, btnY),
            Size = new Size(100, 35),
            Text = "清空",
            Type = TTypeMini.Default
        };
        btnClear.Click += (s, e) => _viewModel.ClearFormCommand.Execute(null);

        btnY += 45;

        btnDelete = new AntdUI.Button
        {
            Location = new Point(10, btnY),
            Size = new Size(100, 35),
            Text = "删除",
            Type = TTypeMini.Error
        };
        btnDelete.Click += (s, e) =>
        {
            if (MessageBox.Show("确定要删除该账户吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _viewModel.DeleteAccountCommand.Execute(null);
            }
        };

        btnToggleFreeze = new AntdUI.Button
        {
            Location = new Point(120, btnY),
            Size = new Size(100, 35),
            Text = "冻结/解冻",
            Type = TTypeMini.Warn
        };
        btnToggleFreeze.Click += (s, e) => _viewModel.ToggleFreezeCommand.Execute(null);

        btnResetPassword = new AntdUI.Button
        {
            Location = new Point(230, btnY),
            Size = new Size(100, 35),
            Text = "重置密码",
            Type = TTypeMini.Warn
        };
        btnResetPassword.Click += (s, e) =>
        {
            if (MessageBox.Show("确定要重置密码为 123456 吗？", "确认重置", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _viewModel.ResetPasswordCommand.Execute(null);
            }
        };

        // 组装控件
        pnlTop.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnRefresh });
        pnlLeft.Controls.Add(tblAccounts);
        pnlRight.Controls.AddRange(new Control[] {
            lblEditTitle, lblUsername, txtUsername, lblPassword, txtPassword,
            lblNickname, txtNickname, lblRole, selRole, lblStatus, swStatus,
            lblRemark, txtRemark, btnAdd, btnUpdate, btnClear, btnDelete,
            btnToggleFreeze, btnResetPassword
        });

        pnlMain.Controls.AddRange(new Control[] { pnlRight, pnlLeft });

        Controls.AddRange(new Control[] { pnlMain, pnlTop });
        BackColor = Color.FromArgb(240, 242, 245);
        Padding = new Padding(10);
    }

    /// <summary>
    /// 初始化数据绑定
    /// </summary>
    private void InitializeDataBindings()
    {
        _viewModel.PropertyChanged += (s, e) =>
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

    /// <summary>
    /// 处理属性变化
    /// </summary>
    private void HandlePropertyChanged(string? propertyName)
    {
        switch (propertyName)
        {
            case nameof(_viewModel.Accounts):
                UpdateAccountsTable();
                break;
            case nameof(_viewModel.Roles):
                UpdateRolesDropdown();
                break;
            case nameof(_viewModel.EditUsername):
                if (txtUsername.Text != _viewModel.EditUsername)
                    txtUsername.Text = _viewModel.EditUsername;
                break;
            case nameof(_viewModel.EditNickname):
                if (txtNickname.Text != _viewModel.EditNickname)
                    txtNickname.Text = _viewModel.EditNickname;
                break;
            case nameof(_viewModel.EditPassword):
                if (txtPassword.Text != _viewModel.EditPassword)
                    txtPassword.Text = _viewModel.EditPassword;
                break;
            case nameof(_viewModel.EditStatus):
                swStatus.Checked = _viewModel.EditStatus;
                break;
            case nameof(_viewModel.EditRemark):
                if (txtRemark.Text != _viewModel.EditRemark)
                    txtRemark.Text = _viewModel.EditRemark;
                break;
        }
    }

    /// <summary>
    /// 更新账户表格
    /// </summary>
    private void UpdateAccountsTable()
    {
        var data = _viewModel.Accounts.Select(a => new
        {
            a.SysId,
            a.SysAccountName,
            a.SysNickname,
            RoleName = "角色",  // TODO: 需要关联查询角色信息
            StatusText = a.SysStatus ?? false ? "启用" : "冻结"
        }).ToArray();

        tblAccounts.DataSource = data;
    }

    /// <summary>
    /// 更新角色下拉框
    /// </summary>
    private void UpdateRolesDropdown()
    {
        selRole.Items.Clear();
        foreach (var role in _viewModel.Roles)
        {
            selRole.Items.Add(new SelectItem(role.SrName ?? "", role.SrId));
        }
    }

    /// <summary>
    /// 表格单元格点击
    /// </summary>
    private void OnTableCellClick(object? sender, TableClickEventArgs e)
    {
        if (e.Record is { } && e.Record.GetType().GetProperty("SysId")?.GetValue(e.Record) is long accountId)
        {
            var account = _viewModel.Accounts.FirstOrDefault(a => a.SysId == accountId);
            if (account != null)
            {
                _viewModel.SelectedAccount = account;
            }
        }
    }

    /// <summary>
    /// 操作成功处理
    /// </summary>
    private void OnOperationSucceeded(object? sender, string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => OnOperationSucceeded(sender, message));
            return;
        }

        MessageBox.Show(message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>
    /// 操作失败处理
    /// </summary>
    private void OnOperationFailed(object? sender, string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => OnOperationFailed(sender, message));
            return;
        }

        MessageBox.Show(message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    /// <summary>
    /// 控件卸载时清理
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _viewModel.OperationSucceeded -= OnOperationSucceeded;
            _viewModel.OperationFailed -= OnOperationFailed;
        }
        base.Dispose(disposing);
    }
}
