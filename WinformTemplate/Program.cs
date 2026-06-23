using Microsoft.Extensions.DependencyInjection;
using System;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Business.Sys.Repositories;
using WinformTemplate.Business.Sys.Service;
using WinformTemplate.Business.Sys.ViewModel;
using WinformTemplate.Business.Template.Context;
using WinformTemplate.Business.Template.Repositories;
using WinformTemplate.Business.Template.Service;
using WinformTemplate.Business.Template.Service.Interface;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Navigation;
using WinformTemplate.Serialize;
using WinformTemplate.UI.Business.Sys.Login;
using WinformTemplate.UI.Business.Sys.Role;
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
        /// 配置依赖注入
        /// </summary>
        private static void ConfigureServices()
        {
            var services = new ServiceCollection();
            var dataSourceOptions = DataSourceOptions.FromConfig(GlobalProjectConfig.Instance.Config);
            services.AddDataSourceOptions(dataSourceOptions);

            // 注册数据库上下文
            bool isDevelopment = true;
            if (dataSourceOptions.Resolve("Sys") == DataSourceKind.Ef)
            {
                SysDbContextService.AddSysDatabase(services, isDevelopment, isDevelopment);
            }

            if (dataSourceOptions.Resolve("Template") == DataSourceKind.Ef)
            {
                TemplateDbContextService.AddTemplateDatabase(services, isDevelopment, isDevelopment);
            }

            // 注册仓储
            RegisterRepositories(services);
            RegisterNavigation(services);

            // 注册业务服务
            RegisterServices(services);

            // 构建服务提供者
            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// 注册仓储
        /// </summary>
        private static void RegisterRepositories(IServiceCollection services)
        {
            services.AddSingleton<IWebApiClient>(_ =>
            {
                var config = GlobalProjectConfig.Instance.Config?.WebApi;
                var client = new WebApiClient();
                client.SetBaseUrl(config?.BaseUrl ?? "https://localhost:5001");
                client.SetTimeout(config?.TimeoutSeconds ?? 30);
                return client;
            });

            // 注册系统仓储
            services.AddModuleRepository<ISysAccountRepository, EfSysAccountRepository, ApiSysAccountRepository, LocalSysAccountRepository>("Sys");
            services.AddModuleRepository<ISysMenuRepository, EfSysMenuRepository, ApiSysMenuRepository, LocalSysMenuRepository>("Sys");
            services.AddModuleRepository<ISysRoleRepository, EfSysRoleRepository, ApiSysRoleRepository, LocalSysRoleRepository>("Sys");
            services.AddModuleRepository<ISysParamRepository, EfSysParamRepository, ApiSysParamRepository, LocalSysParamRepository>("Sys");
            services.AddModuleRepository<ISysRoleAuthRepository, EfSysRoleAuthRepository, ApiSysRoleAuthRepository, LocalSysRoleAuthRepository>("Sys");

            // 注册模板仓储
            services.AddModuleRepository<IProductRepository, EfProductRepository, ApiProductRepository, LocalProductRepository>("Template");
            services.AddModuleRepository<ICategoryRepository, EfCategoryRepository, ApiCategoryRepository, LocalCategoryRepository>("Template");
            services.AddModuleRepository<IImportRecordRepository, EfImportRecordRepository, ApiImportRecordRepository, LocalImportRecordRepository>("Template");
        }

        private static void RegisterNavigation(IServiceCollection services)
        {
            var pageRegistry = new PageRegistry();
            pageRegistry.RegisterDefaultPages();

            services.AddSingleton<IPageRegistry>(pageRegistry);
            services.AddScoped<ICurrentAccountAccessor, CurrentAccountAccessor>();
            services.AddScoped<INavigationService, NavigationService>();
            services.AddTransient<RolePlaceholderControl>();
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
            services.AddScoped<LoginViewModel>();
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

            using var scope = _serviceProvider.CreateScope();
            var initializers = scope.ServiceProvider.GetServices<IDatabaseInitializer>();
            foreach (var initializer in initializers)
            {
                await initializer.InitializeAsync();
                Debug.Info($"{initializer.ModuleKey} 数据库初始化完成");
            }

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
                var loginForm = scope.ServiceProvider.GetRequiredService<LoginForm>();
                var loginViewModel = scope.ServiceProvider.GetRequiredService<LoginViewModel>();

                if (loginForm.ShowDialog() == DialogResult.OK && loginViewModel.CurrentAccount != null)
                {
                    Debug.Info($"登录成功，打开主界面: {loginViewModel.CurrentAccount.SysAccountName}");

                    // 创建新的 scope 用于主窗体
                    var mainScope = _serviceProvider.CreateScope();
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
