using Microsoft.Extensions.DependencyInjection;
using System;
using WinformTemplate.Business.Sys.Repositories;
using WinformTemplate.Business.Sys.Service;
using WinformTemplate.Business.Sys.Service.Full;
using WinformTemplate.Business.Sys.ViewModel;
using WinformTemplate.Business.Template.Repositories;
using WinformTemplate.Business.Template.Repositories.Interface;
using WinformTemplate.Business.Template.Service;
using WinformTemplate.Business.Template.Service.Interface;
using WinformTemplate.Serialize;
using WinformTemplate.UI.Business.Sys.Login;
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

            // 注册数据库上下文
            bool isDevelopment = true;
            SysDbContextService.AddSysDatabase(services, isDevelopment, isDevelopment);
            TemplateDbContextService.AddTemplateDatabase(services, isDevelopment, isDevelopment);

            // 注册仓储
            RegisterRepositories(services);

            // 注册业务服务
            RegisterServices(services);

            // 注册窗体
            services.AddTransient<MainForm>();

            // 构建服务提供者
            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// 注册仓储
        /// </summary>
        private static void RegisterRepositories(IServiceCollection services)
        {
            // 注册系统仓储
            services.AddScoped<ISysAccountRepository, SysAccountRepository>();
            services.AddScoped<ISysMenuRepository, SysMenuRepository>();
            services.AddScoped<ISysRoleRepository, SysRoleRepository>();
            services.AddScoped<ISysParamRepository, SysParamRepository>();
            services.AddScoped<ISysRoleAuthRepository, SysRoleAuthRepository>();

            // 注册模板仓储
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IImportRecordRepository, ImportRecordRepository>();
        }

        /// <summary>
        /// 注册业务服务
        /// </summary>
        private static void RegisterServices(IServiceCollection services)
        {
            // 注册系统业务服务
            services.AddScoped<ISysAccountService, SysAccountService>();
            services.AddScoped<ISysRoleService, SysRoleService>();
            services.AddScoped<ISysMenuService, SysMenuService>();
            services.AddScoped<ISysParamService, SysParamService>();
            services.AddScoped<IPermissionService, PermissionService>();

            // 注册模板业务服务
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();

            // 注册系统 ViewModel
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<AccountManagementViewModel>();

            // 注册系统窗体
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

            // 初始化系统数据库
            var sysDbService = _serviceProvider.GetRequiredService<SysDbContextService>();
            await sysDbService.EnsureDatabaseCreatedAsync();
            await sysDbService.InitializeDatabaseAsync();
            Debug.Info("系统数据库初始化完成");

            // 初始化模板数据库
            var templateDbService = _serviceProvider.GetRequiredService<TemplateDbContextService>();
            await templateDbService.EnsureDatabaseCreatedAsync();
            await templateDbService.InitializeDatabaseAsync();
            Debug.Info("模板数据库初始化完成");

            Debug.Info("所有数据库初始化完成");
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