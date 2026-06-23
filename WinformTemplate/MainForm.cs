using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Business.Sys.ViewModel;
using WinformTemplate.Logger;
using WinformTemplate.Navigation;
using WinformTemplate.UIComponent;

namespace WinformTemplate
{
    public partial class MainForm : Form
    {
        private readonly LabelWriter _labelWriter;
        private readonly MainViewModel? _viewModel;
        private readonly INavigationService? _navigationService;
        private readonly ICurrentAccountAccessor? _currentAccountAccessor;
        private bool _updatingTabs;

        public MainForm(
            MainViewModel? viewModel = null,
            INavigationService? navigationService = null,
            ICurrentAccountAccessor? currentAccountAccessor = null)
        {
            InitializeComponent();

            _labelWriter = new LabelWriter(Lab_Console);
            Console.SetOut(_labelWriter);

            LoadBaseInfo();

            _viewModel = viewModel;
            _navigationService = navigationService;
            _currentAccountAccessor = currentAccountAccessor;

            if (_viewModel != null)
            {
                InitializeViewModel();
            }
        }

        private void InitializeViewModel()
        {
            if (_viewModel == null) return;

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            _viewModel.MenuSelected += ViewModel_MenuSelected;
            _viewModel.LogoutRequested += ViewModel_LogoutRequested;
            _viewModel.LoadMenusCommand.Execute(null);
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.MenuItems))
            {
                UpdateMenuTabs();
            }
        }

        private void ViewModel_MenuSelected(object? sender, SysMenuModel menu)
        {
            Debug.Info($"Menu selected: {menu.SmName}, Url={menu.SmUrl}");
        }

        private void ViewModel_LogoutRequested(object? sender, EventArgs e)
        {
            Debug.Info("Logout requested.");
            Close();
        }

        private void UpdateMenuTabs()
        {
            if (_viewModel == null) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(UpdateMenuTabs));
                return;
            }

            _updatingTabs = true;
            try
            {
                Tab_Main.Pages.Clear();

                foreach (var menu in GetRoutableMenus(_viewModel.MenuItems))
                {
                    Tab_Main.Pages.Add(new AntdUI.TabPage
                    {
                        Text = menu.SmName,
                        Tag = menu
                    });
                }

                if (Tab_Main.Pages.Count == 0)
                {
                    var emptyPage = new AntdUI.TabPage
                    {
                        Text = "无可用菜单"
                    };
                    AddControlToPage(emptyPage, NavigationPlaceholderPage.AccessDenied("任何菜单"));
                    Tab_Main.Pages.Add(emptyPage);
                    return;
                }

                Tab_Main.SelectedIndex = 0;
            }
            finally
            {
                _updatingTabs = false;
            }

            _ = LoadSelectedTabAsync();
        }

        public void SetCurrentAccount(SysAccountModel account)
        {
            if (_currentAccountAccessor != null)
            {
                _currentAccountAccessor.CurrentAccount = account;
            }

            if (_viewModel != null)
            {
                _viewModel.CurrentAccount = account;
            }

            Text = $"{Application.ProductVersion} - 当前用户: {account.SysAccountName}";
        }

        private void LoadBaseInfo()
        {
            Text = $"{Text} --- {Application.ProductVersion}";
        }

        private async void Tab_Main_SelectedIndexChanged(object sender, AntdUI.IntEventArgs e)
        {
            if (_updatingTabs)
            {
                return;
            }

            await LoadSelectedTabAsync();
        }

        private async Task LoadSelectedTabAsync()
        {
            var selectedPage = Tab_Main.SelectedTab;
            if (selectedPage?.Tag is not SysMenuModel menu)
            {
                return;
            }

            if (selectedPage.Controls.Count > 0)
            {
                return;
            }

            _viewModel?.SelectMenu(menu);
            if (_navigationService == null)
            {
                AddControlToPage(selectedPage, NavigationPlaceholderPage.NotImplemented(menu.SmUrl));
                return;
            }

            var result = await _navigationService.NavigateAsync(menu.SmUrl);
            AddControlToPage(selectedPage, result.Page);
            Debug.Info($"Loaded navigation result: MenuKey={result.MenuKey}, Status={result.Status}");
        }

        private void AddControlToPage(AntdUI.TabPage tabPage, UserControl userControl)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AddControlToPage(tabPage, userControl)));
                return;
            }

            tabPage.Controls.Clear();
            userControl.Dock = DockStyle.Fill;
            tabPage.Controls.Add(userControl);
        }

        private static IEnumerable<SysMenuModel> GetRoutableMenus(IEnumerable<SysMenuModel> menus)
        {
            return menus
                .Where(menu => !string.IsNullOrWhiteSpace(menu.SmUrl) && menu.SmUrl != "#")
                .GroupBy(menu => menu.SmUrl, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(menu => menu.SmLevel ?? 0)
                .ThenBy(menu => menu.SmSort ?? int.MaxValue)
                .ThenBy(menu => menu.SmId);
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                return;
            }

            var dis = Math.Abs(Size.Height - Lab_Console.Size.Height);
            var newSplitterDistance = Math.Max(
                SContainer_Main.Panel1MinSize,
                Math.Min(SContainer_Main.Width - SContainer_Main.Panel2MinSize, dis));

            SContainer_Main.SplitterDistance = newSplitterDistance;
        }

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
