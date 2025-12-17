using AntdUI;
using WinformTemplate.Business.Sys.ViewModel;

namespace WinformTemplate.Business.Sys.View;

/// <summary>
/// 登录窗体
/// </summary>
public partial class LoginForm : Window
{
    private readonly LoginViewModel _viewModel;

    // 控件
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

        // 订阅登录成功事件
        _viewModel.LoginSucceeded += OnLoginSucceeded;
    }

    private void InitializeComponent()
    {
        // 窗体设置
        Text = "系统登录";
        Size = new Size(450, 350);
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        BackColor = Color.White;

        // 标题
        lblTitle = new System.Windows.Forms.Label
        {
            Text = "欢迎登录",
            Font = new Font("Microsoft YaHei UI", 20F, FontStyle.Bold),
            ForeColor = Color.FromArgb(22, 119, 255),
            Location = new Point(0, 40),
            Size = new Size(450, 40),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // 用户名标签
        lblUsername = new System.Windows.Forms.Label
        {
            Text = "用户名",
            Font = new Font("Microsoft YaHei UI", 10F),
            Location = new Point(70, 110),
            Size = new Size(60, 25),
            TextAlign = ContentAlignment.MiddleLeft
        };

        // 用户名输入框
        txtUsername = new Input
        {
            Location = new Point(70, 140),
            Size = new Size(310, 40),
            PlaceholderText = "请输入用户名",
            Font = new Font("Microsoft YaHei UI", 10F)
        };
        txtUsername.TextChanged += (s, e) =>
        {
            _viewModel.Username = txtUsername.Text;
        };

        // 密码标签
        lblPassword = new System.Windows.Forms.Label
        {
            Text = "密码",
            Font = new Font("Microsoft YaHei UI", 10F),
            Location = new Point(70, 190),
            Size = new Size(60, 25),
            TextAlign = ContentAlignment.MiddleLeft
        };

        // 密码输入框
        txtPassword = new Input
        {
            Location = new Point(70, 220),
            Size = new Size(310, 40),
            PlaceholderText = "请输入密码",
            Font = new Font("Microsoft YaHei UI", 10F),
            UseSystemPasswordChar = true
        };
        txtPassword.TextChanged += (s, e) =>
        {
            _viewModel.Password = txtPassword.Text;
        };
        txtPassword.KeyPress += (s, e) =>
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                ExecuteLogin();
            }
        };

        // 错误消息标签
        lblError = new System.Windows.Forms.Label
        {
            Text = "",
            Font = new Font("Microsoft YaHei UI", 9F),
            ForeColor = Color.FromArgb(255, 77, 79),
            Location = new Point(70, 265),
            Size = new Size(310, 20),
            TextAlign = ContentAlignment.MiddleLeft
        };

        // 登录按钮
        btnLogin = new AntdUI.Button
        {
            Text = "登 录",
            Location = new Point(70, 290),
            Size = new Size(310, 40),
            Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold),
            Type = TTypeMini.Primary,
            BorderWidth = 0,
            Radius = 6
        };
        btnLogin.Click += (s, e) => ExecuteLogin();

        // 添加控件到窗体
        Controls.Add(lblTitle);
        Controls.Add(lblUsername);
        Controls.Add(txtUsername);
        Controls.Add(lblPassword);
        Controls.Add(txtPassword);
        Controls.Add(lblError);
        Controls.Add(btnLogin);
    }

    /// <summary>
    /// 初始化数据绑定
    /// </summary>
    private void InitializeDataBindings()
    {
        // 监听 ViewModel 属性变化
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.ErrorMessage))
            {
                UpdateErrorMessage();
            }
            else if (e.PropertyName == nameof(_viewModel.IsBusy))
            {
                UpdateButtonState();
            }
        };
    }

    /// <summary>
    /// 更新错误消息显示
    /// </summary>
    private void UpdateErrorMessage()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(UpdateErrorMessage));
            return;
        }

        lblError.Text = _viewModel.ErrorMessage;
    }

    /// <summary>
    /// 更新按钮状态
    /// </summary>
    private void UpdateButtonState()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(UpdateButtonState));
            return;
        }

        btnLogin.Enabled = !_viewModel.IsBusy;
        btnLogin.Loading = _viewModel.IsBusy;

        if (_viewModel.IsBusy)
        {
            btnLogin.Text = "登录中...";
        }
        else
        {
            btnLogin.Text = "登 录";
        }
    }

    /// <summary>
    /// 执行登录
    /// </summary>
    private void ExecuteLogin()
    {
        if (_viewModel.LoginCommand.CanExecute(null))
        {
            _viewModel.LoginCommand.Execute(null);
        }
    }

    /// <summary>
    /// 登录成功处理
    /// </summary>
    private void OnLoginSucceeded(object? sender, Business.Sys.Model.SysAccountModel account)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => OnLoginSucceeded(sender, account)));
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    /// <summary>
    /// 窗体关闭时清理资源
    /// </summary>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _viewModel.LoginSucceeded -= OnLoginSucceeded;
        base.OnFormClosing(e);
    }
}
