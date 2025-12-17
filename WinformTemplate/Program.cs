using Microsoft.Extensions.DependencyInjection;
using System;
using WinformTemplate.Business.Sys.Repositories;
using WinformTemplate.Business.Sys.Service;
using WinformTemplate.Business.Sys.Service.Full;
using WinformTemplate.Business.Sys.View;
using WinformTemplate.Business.Sys.ViewModel;
using WinformTemplate.Serialize;
using Debug = WinformTemplate.Logger.Debug;

namespace WinformTemplate
{
    internal static class Program
    {
        private static IServiceProvider? _serviceProvider;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Load log4net system
            Debug.InitLog4Net();
            Debug.Info("Project start: " + Application.ProductVersion);

            // Load Config system
            GlobalProjectConfig.Instance.CheckConfigLoaded();

            // Configure dependency injection
            ConfigureServices();

            // Initialize database
            InitializeDatabaseAsync().GetAwaiter().GetResult();

            // Show login form
            ShowLoginForm();
        }

        /// <summary>
        /// ��������ע�����
        /// </summary>
        private static void ConfigureServices()
        {
            var services = new ServiceCollection();

            // ע�����ݿ���� (��������������ϸ����)
            bool isDevelopment = true; // ��ʵ��Ӧ���п��Դ������ļ���ȡ
            SysDbContextService.AddSysDatabase(services, isDevelopment, isDevelopment);

            // ע��ִ�
            RegisterRepositories(services);

            // ע��ҵ�����
            RegisterServices(services);

            // ע�ᴰ��
            services.AddTransient<MainForm>();

            // ���������ṩ��
            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// 注册仓储
        /// </summary>
        private static void RegisterRepositories(IServiceCollection services)
        {
            // 注册各个仓储实现
            services.AddScoped<ISysAccountRepository, SysAccountRepository>();
            services.AddScoped<ISysMenuRepository, SysMenuRepository>();
            services.AddScoped<ISysRoleRepository, SysRoleRepository>();
            services.AddScoped<ISysParamRepository, SysParamRepository>();
            services.AddScoped<ISysRoleAuthRepository, SysRoleAuthRepository>();
        }

        /// <summary>
        /// 注册业务服务
        /// </summary>
        private static void RegisterServices(IServiceCollection services)
        {
            // 注册业务服务
            services.AddScoped<ISysAccountService, SysAccountService>();
            services.AddScoped<ISysRoleService, SysRoleService>();
            services.AddScoped<ISysMenuService, SysMenuService>();
            services.AddScoped<ISysParamService, SysParamService>();
            services.AddScoped<IPermissionService, PermissionService>();

            // 注册 ViewModel
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<AccountManagementViewModel>();

            // 注册窗体
            services.AddTransient<LoginForm>();
            services.AddTransient<MainForm>();
            services.AddTransient<AccountManagementControl>();
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        private static async Task InitializeDatabaseAsync()
        {
            if (_serviceProvider == null) return;

            var dbService = _serviceProvider.GetRequiredService<SysDbContextService>();

            // 确保数据库创建
            await dbService.EnsureDatabaseCreatedAsync();

            // 初始化种子数据
            await dbService.InitializeDatabaseAsync();

            Debug.Info("数据库初始化完成");
        }

        /// <summary>
        /// 显示登录窗体
        /// </summary>
        private static void ShowLoginForm()
        {
            if (_serviceProvider == null)
            {
                Debug.Error("ServiceProvider 未初始化");
                return;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var loginViewModel = scope.ServiceProvider.GetRequiredService<LoginViewModel>();
                var loginForm = new LoginForm(loginViewModel);

                if (loginForm.ShowDialog() == DialogResult.OK && loginViewModel.CurrentAccount != null)
                {
                    Debug.Info($"登录成功，打开主界面: {loginViewModel.CurrentAccount.SysAccountName}");

                    // 创建新的 scope 用于主窗体
                    var mainScope = _serviceProvider.CreateScope();
                    var mainViewModel = mainScope.ServiceProvider.GetRequiredService<MainViewModel>();
                    var mainForm = mainScope.ServiceProvider.GetRequiredService<MainForm>();

                    // 设置当前账户
                    mainForm.SetCurrentAccount(loginViewModel.CurrentAccount);

                    Application.Run(mainForm);

                    mainScope.Dispose();
                }
                else
                {
                    Debug.Info("用户取消登录或登录失败");
                }
            }
        }
    }
}