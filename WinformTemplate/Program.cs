using Microsoft.Extensions.DependencyInjection;
using System;
using WinformTemplate.Business.Sys.Repositories;
using WinformTemplate.Business.Sys.Service.Full;
using WinformTemplate.Serialize;
using Debug = WinformTemplate.Logger.Debug;

namespace WinformTemplate
{
    internal static class Program
    {
        private static IServiceProvider _serviceProvider;

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

            // Load Locker system

            Application.Run(new MainForm());
        }

        /// <summary>
        /// 配置依赖注入服务
        /// </summary>
        private static void ConfigureServices()
        {
            var services = new ServiceCollection();

            // 注册数据库服务 (开发环境启用详细错误)
            bool isDevelopment = true; // 在实际应用中可以从配置文件读取
            SysDbContextService.AddSysDatabase(services, isDevelopment, isDevelopment);

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
        /// <param name="services">服务集合</param>
        private static void RegisterRepositories(IServiceCollection services)
        {
            // 注册具体仓储实现
            services.AddScoped<ISysAccountRepository, SysAccountRepository>();
            services.AddScoped<ISysMenuRepository, SysMenuRepository>();
            services.AddScoped<ISysRoleRepository, SysRoleRepository>();
            services.AddScoped<ISysParamRepository, SysParamRepository>();
        }

        /// <summary>
        /// 注册业务服务
        /// </summary>
        /// <param name="services">服务集合</param>
        private static void RegisterServices(IServiceCollection services)
        {
            // 注册业务服务
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IMenuService, MenuService>();
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        private static async Task InitializeDatabaseAsync()
        {
            var dbService = _serviceProvider.GetRequiredService<SysDbContextService>();

            // 确保数据库创建
            await dbService.EnsureDatabaseCreatedAsync();

            // 初始化种子数据
            await dbService.InitializeDatabaseAsync();
        }
    }
}