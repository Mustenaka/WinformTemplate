using AntdUI;
using WinformTemplate.Business.Sys.ViewModel;
using WinformTemplate.Common.MVVM.Extensions;

namespace WinformTemplate.UI.Business.Sys.Login;

/// <summary>
/// 登录窗体。
/// </summary>
public partial class LoginForm : Window
{
    private readonly LoginViewModel _viewModel;

    private System.Windows.Forms.Label lblTitle = null!;
    private System.Windows.Forms.Label lblUsername = null!;
    private Input txtUsername = null!;
    private System.Windows.Forms.Label lblPassword = null!;
    private Input txtPassword = null!;
    private System.Windows.Forms.Label lblError = null!;
    private AntdUI.Button btnLogin = null!;

    public LoginForm(LoginViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        InitializeComponent();
        InitializeDataBindings();

        _viewModel.LoginSucceeded += OnLoginSucceeded;
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        Text = "系统登录";
        Size = new Size(460, 390);
        MinimumSize = new Size(460, 390);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        BackColor = Color.White;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(44, 28, 44, 28),
            ColumnCount = 3,
            RowCount = 11
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 12F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        lblTitle = new System.Windows.Forms.Label
        {
            Text = "欢迎登录",
            Dock = DockStyle.Fill,
            Font = new Font("Microsoft YaHei UI", 20F, FontStyle.Bold),
            ForeColor = Color.FromArgb(22, 119, 255),
            TextAlign = ContentAlignment.MiddleCenter
        };

        lblUsername = CreateFieldLabel("用户名");
        txtUsername = CreateInput("请输入用户名");
        txtUsername.KeyPress += (_, e) =>
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                txtPassword.Focus();
            }
        };

        lblPassword = CreateFieldLabel("密码");
        txtPassword = CreateInput("请输入密码");
        txtPassword.UseSystemPasswordChar = true;
        txtPassword.KeyPress += (_, e) =>
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                ExecuteLogin();
            }
        };

        lblError = new System.Windows.Forms.Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Microsoft YaHei UI", 9F),
            ForeColor = Color.FromArgb(255, 77, 79),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };

        btnLogin = new AntdUI.Button
        {
            Text = "登录",
            Dock = DockStyle.Fill,
            Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold),
            Type = TTypeMini.Primary,
            BorderWidth = 0,
            Radius = 6
        };

        root.Controls.Add(lblTitle, 1, 1);
        root.Controls.Add(lblUsername, 1, 3);
        root.Controls.Add(txtUsername, 1, 4);
        root.Controls.Add(lblPassword, 1, 6);
        root.Controls.Add(txtPassword, 1, 7);
        root.Controls.Add(lblError, 1, 8);
        root.Controls.Add(btnLogin, 1, 9);

        Controls.Add(root);

        ResumeLayout(false);
    }

    private void InitializeDataBindings()
    {
        txtUsername.BindText(_viewModel, nameof(LoginViewModel.Username));
        txtPassword.BindText(_viewModel, nameof(LoginViewModel.Password));
        btnLogin.BindCommand(_viewModel.LoginCommand);

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(LoginViewModel.ErrorMessage))
            {
                UpdateErrorMessage();
            }
            else if (e.PropertyName == nameof(LoginViewModel.IsBusy))
            {
                UpdateButtonState();
            }
        };
    }

    private void UpdateErrorMessage()
    {
        if (InvokeRequired)
        {
            Invoke((MethodInvoker)UpdateErrorMessage);
            return;
        }

        lblError.Text = _viewModel.ErrorMessage;
    }

    private void UpdateButtonState()
    {
        if (InvokeRequired)
        {
            Invoke((MethodInvoker)UpdateButtonState);
            return;
        }

        btnLogin.Enabled = !_viewModel.IsBusy;
        btnLogin.Loading = _viewModel.IsBusy;
        btnLogin.Text = _viewModel.IsBusy ? "登录中..." : "登录";
    }

    private void ExecuteLogin()
    {
        if (_viewModel.LoginCommand.CanExecute(null))
        {
            _viewModel.LoginCommand.Execute(null);
        }
    }

    private void OnLoginSucceeded(object? sender, WinformTemplate.Business.Sys.Model.SysAccountModel account)
    {
        if (InvokeRequired)
        {
            Invoke((MethodInvoker)(() => OnLoginSucceeded(sender, account)));
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _viewModel.LoginSucceeded -= OnLoginSucceeded;
        base.OnFormClosing(e);
    }

    private static System.Windows.Forms.Label CreateFieldLabel(string text)
    {
        return new System.Windows.Forms.Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = new Font("Microsoft YaHei UI", 10F),
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static Input CreateInput(string placeholder)
    {
        return new Input
        {
            Dock = DockStyle.Fill,
            PlaceholderText = placeholder,
            Font = new Font("Microsoft YaHei UI", 10F)
        };
    }
}
