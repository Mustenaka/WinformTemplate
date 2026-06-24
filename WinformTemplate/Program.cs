using Microsoft.Extensions.DependencyInjection;
using WinformTemplate.Bootstrap;
using WinformTemplate.Business.Sys.ViewModel;
using WinformTemplate.Serialize;
using WinformTemplate.UI.Business.Sys.Login;
using Debug = WinformTemplate.Logger.Debug;

namespace WinformTemplate;

internal static class Program
{
    private static IServiceProvider? _serviceProvider;

    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        Debug.InitLog4Net();
        Debug.Info("Project start: " + Application.ProductVersion);

        try
        {
            GlobalProjectConfig.Instance.CheckConfigLoaded();
            ConfigureServices();
            InitializeDatabaseAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Debug.Fatal("Application initialization failed", ex);
            MessageBox.Show(
                $"应用初始化失败，程序将退出。\n\n{ex.Message}",
                "初始化失败",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        ShowLoginForm();
    }

    private static void ConfigureServices()
    {
        _serviceProvider = AppServiceRegistration.BuildServiceProvider(GlobalProjectConfig.Instance.Config, isDevelopment: true);
    }

    private static async Task InitializeDatabaseAsync()
    {
        await AppServiceRegistration.InitializeDatabaseAsync(_serviceProvider);
    }

    private static void ShowLoginForm()
    {
        if (_serviceProvider == null)
        {
            Debug.Error("ServiceProvider is not initialized.");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var loginForm = scope.ServiceProvider.GetRequiredService<LoginForm>();
        var loginViewModel = scope.ServiceProvider.GetRequiredService<LoginViewModel>();

        if (loginForm.ShowDialog() != DialogResult.OK || loginViewModel.CurrentAccount == null)
        {
            Debug.Info("User cancelled login or login failed.");
            return;
        }

        Debug.Info($"Login succeeded, opening main form: {loginViewModel.CurrentAccount.SysAccountName}");

        using var mainScope = _serviceProvider.CreateScope();
        var mainForm = mainScope.ServiceProvider.GetRequiredService<MainForm>();
        mainForm.SetCurrentAccount(loginViewModel.CurrentAccount);

        Application.Run(mainForm);
    }
}
