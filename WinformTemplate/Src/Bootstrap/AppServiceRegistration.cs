using Microsoft.Extensions.DependencyInjection;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Business.Sys.Repositories;
using WinformTemplate.Business.Sys.Service;
using WinformTemplate.Business.Sys.ViewModel;
using WinformTemplate.Business.Template.Context;
using WinformTemplate.Business.Template.Repositories;
using WinformTemplate.Business.Template.Service;
using WinformTemplate.Business.Template.Service.Interface;
using WinformTemplate.Business.Template.ViewModel;
using WinformTemplate.Common.Config;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Navigation;
using WinformTemplate.Serialize;
using WinformTemplate.UI.Business.Sys.Login;
using WinformTemplate.UI.Business.Sys.Role;
using WinformTemplate.UI.Business.Template.Product;
using Debug = WinformTemplate.Logger.Debug;

namespace WinformTemplate.Bootstrap;

public static class AppServiceRegistration
{
    public static IServiceProvider BuildServiceProvider(ProjectConfig? config, bool isDevelopment = true)
    {
        var services = new ServiceCollection();
        ConfigureServices(services, config, isDevelopment);
        return services.BuildServiceProvider();
    }

    public static void ConfigureServices(IServiceCollection services, ProjectConfig? config, bool isDevelopment = true)
    {
        ArgumentNullException.ThrowIfNull(services);

        GlobalProjectConfig.Instance.Config = config ?? new ProjectConfig();
        var dataSourceOptions = DataSourceOptions.FromConfig(GlobalProjectConfig.Instance.Config);
        services.AddDataSourceOptions(dataSourceOptions);

        if (dataSourceOptions.Resolve("Sys") == DataSourceKind.Ef)
        {
            SysDbContextService.AddSysDatabase(services, isDevelopment, isDevelopment);
        }

        if (dataSourceOptions.Resolve("Template") == DataSourceKind.Ef)
        {
            TemplateDbContextService.AddTemplateDatabase(services, isDevelopment, isDevelopment);
        }

        RegisterRepositories(services);
        RegisterNavigation(services);
        RegisterServices(services);
    }

    public static async Task InitializeDatabaseAsync(IServiceProvider? serviceProvider, CancellationToken cancellationToken = default)
    {
        if (serviceProvider == null)
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var initializers = scope.ServiceProvider.GetServices<IDatabaseInitializer>();
        foreach (var initializer in initializers)
        {
            await initializer.InitializeAsync(cancellationToken);
            Debug.Info($"{initializer.ModuleKey} database initialized");
        }

        Debug.Info("Database initialization completed");
    }

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

        services.AddModuleRepository<ISysAccountRepository, EfSysAccountRepository, ApiSysAccountRepository, LocalSysAccountRepository>("Sys");
        services.AddModuleRepository<ISysMenuRepository, EfSysMenuRepository, ApiSysMenuRepository, LocalSysMenuRepository>("Sys");
        services.AddModuleRepository<ISysRoleRepository, EfSysRoleRepository, ApiSysRoleRepository, LocalSysRoleRepository>("Sys");
        services.AddModuleRepository<ISysParamRepository, EfSysParamRepository, ApiSysParamRepository, LocalSysParamRepository>("Sys");
        services.AddModuleRepository<ISysRoleAuthRepository, EfSysRoleAuthRepository, ApiSysRoleAuthRepository, LocalSysRoleAuthRepository>("Sys");

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

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<ISysAccountService, SysAccountService>();
        services.AddScoped<ISysRoleService, SysRoleService>();
        services.AddScoped<ISysMenuService, SysMenuService>();
        services.AddScoped<ISysParamService, SysParamService>();
        services.AddScoped<IPermissionService, PermissionService>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();

        services.AddScoped<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<AccountManagementViewModel>();
        services.AddTransient<ProductManagementViewModel>();

        services.AddTransient<LoginForm>();
        services.AddTransient<MainForm>();
        services.AddTransient<AccountManagementControl>();
        services.AddTransient<ProductManagementControl>();
    }
}
