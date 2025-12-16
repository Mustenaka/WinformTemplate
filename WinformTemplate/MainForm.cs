using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.ViewModel;
using WinformTemplate.Logger;
using WinformTemplate.UIComponent;

namespace WinformTemplate
{
    /// <summary>
    /// 主界面UI类
    /// </summary>
    public partial class MainForm : Form
    {
        // UI 输出窗台
        private readonly LabelWriter? _labelWriter;
        private readonly MainViewModel? _viewModel;

        public MainForm(MainViewModel? viewModel = null)
        {
            InitializeComponent();

            // 如果不存在默认 UI Console
            _labelWriter ??= new LabelWriter(this.Lab_Console);
            Console.SetOut(_labelWriter);

            // 表单加载信息时候显示版本
            LoadBaseInfo();

            // 设置 ViewModel
            _viewModel = viewModel;
            if (_viewModel != null)
            {
                InitializeViewModel();
            }
        }

        /// <summary>
        /// 初始化 ViewModel
        /// </summary>
        private void InitializeViewModel()
        {
            if (_viewModel == null) return;

            // 订阅事件
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            _viewModel.MenuSelected += ViewModel_MenuSelected;
            _viewModel.LogoutRequested += ViewModel_LogoutRequested;

            // 加载菜单
            _viewModel.LoadMenusCommand.Execute(null);
        }

        /// <summary>
        /// ViewModel 属性变化处理
        /// </summary>
        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.MenuItems))
            {
                UpdateMenuTabs();
            }
        }

        /// <summary>
        /// 菜单选择处理
        /// </summary>
        private void ViewModel_MenuSelected(object? sender, SysMenuModel menu)
        {
            Debug.Info($"菜单选择: {menu.SmName}");
            // 这里可以添加打开对应功能页面的逻辑
        }

        /// <summary>
        /// 退出登录处理
        /// </summary>
        private void ViewModel_LogoutRequested(object? sender, EventArgs e)
        {
            Debug.Info("退出登录");
            this.Close();
        }

        /// <summary>
        /// 更新菜单标签页
        /// </summary>
        private void UpdateMenuTabs()
        {
            if (_viewModel == null) return;

            if (InvokeRequired)
            {
                Invoke(new Action(UpdateMenuTabs));
                return;
            }

            // 清空现有标签页
            Tab_Main.Pages.Clear();

            // 根据权限菜单创建标签页
            foreach (var menu in _viewModel.MenuItems)
            {
                // 只显示顶级菜单（父菜单ID为0或null）
                if (menu.SmParentId == 0 || menu.SmParentId == null)
                {
                    var tabPage = new AntdUI.TabPage
                    {
                        Text = menu.SmName,
                        Tag = menu
                    };
                    Tab_Main.Pages.Add(tabPage);
                }
            }
        }

        /// <summary>
        /// 设置当前账户
        /// </summary>
        public void SetCurrentAccount(SysAccountModel account)
        {
            if (_viewModel != null)
            {
                _viewModel.CurrentAccount = account;
            }

            // 更新标题显示用户信息
            this.Text = $"{Application.ProductVersion} - 当前用户: {account.SysAccountName}";
        }

        /// <summary>
        /// 加载窗体时候附加显示信息+版本号
        /// </summary>
        private void LoadBaseInfo()
        {
            var title = this.Text + " --- " + Application.ProductVersion;
            this.Text = title;
        }

        /// <summary>
        /// 切换选项卡
        /// </summary>
        private void Tab_Main_SelectedIndexChanged(object sender, AntdUI.IntEventArgs e)
        {
            var selectPage = this.Tab_Main.SelectedTab;
            var selectIndex = this.Tab_Main.SelectedIndex;

            if (selectPage == null) return;

            // 获取关联的菜单
            var menu = selectPage.Tag as SysMenuModel;
            if (menu != null && _viewModel != null)
            {
                _viewModel.SelectMenu(menu);
            }

            selectPage.Controls.Clear();

            UserControl? userControl = null;

            // 根据菜单ID或名称加载对应的用户控件
            // 这里可以根据实际业务扩展
            switch (selectIndex)
            {
                case 0:
                    break;
                case 1:
                    break;
            }

            if (userControl == null) return;

            userControl.Dock = DockStyle.Fill;
            selectPage.Controls.Add(userControl);

            Debug.Info($"{this.GetType().Name} 切换页面 {selectIndex}");
        }

        /// <summary>
        /// 窗体大小变化时动态调整控制台面板大小以保持一致的响应
        /// </summary>
        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            // 检查窗体是否被最小化
            if (this.WindowState == FormWindowState.Minimized)
            {
                // 窗体最小化时跳过 SplitterDistance 的调整
                return;
            }

            var dis = Math.Abs(this.Size.Height - Lab_Console.Size.Height);

            // 在大小变化的情况下调整 SplitterDistance
            var newSplitterDistance = Math.Max(
                SContainer_Main.Panel1MinSize, Math.Min(SContainer_Main.Width - SContainer_Main.Panel2MinSize, dis)
            );

            SContainer_Main.SplitterDistance = newSplitterDistance;
        }

        #region Function

        /// <summary>
        /// 首次加载页面
        /// </summary>
        private void FirstLoadTabPage(UserControl firstLoad)
        {
            var selectPage = this.Tab_Main.SelectedTab;
            firstLoad.Dock = DockStyle.Fill;
            selectPage?.Controls.Add(firstLoad);
        }

        #endregion

        /// <summary>
        /// 窗体关闭时清理资源
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                _viewModel.MenuSelected -= ViewModel_MenuSelected;
                _viewModel.LogoutRequested -= ViewModel_LogoutRequested;
            }
            base.OnFormClosing(e);
        }
    }
}
