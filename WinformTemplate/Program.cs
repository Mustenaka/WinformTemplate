using Microsoft.Extensions.DependencyInjection;
using System;
using WinformTemplate.Business.Sys.Repositories;
using WinformTemplate.Business.Sys.Service;
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
        /// ע��ִ�
        /// </summary>
        /// <param name="services">���񼯺�</param>
        private static void RegisterRepositories(IServiceCollection services)
        {
            // ע�����ִ�ʵ��
            services.AddScoped<ISysAccountRepository, SysAccountRepository>();
            services.AddScoped<ISysMenuRepository, SysMenuRepository>();
            services.AddScoped<ISysRoleRepository, SysRoleRepository>();
            services.AddScoped<ISysParamRepository, SysParamRepository>();
        }

        /// <summary>
        /// ע��ҵ�����
        /// </summary>
        /// <param name="services">���񼯺�</param>
        private static void RegisterServices(IServiceCollection services)
        {
            // 注册业务服务
            services.AddScoped<ISysAccountService, SysAccountService>();
            services.AddScoped<ISysRoleService, SysRoleService>();
            services.AddScoped<ISysMenuService, SysMenuService>();
            services.AddScoped<ISysParamService, SysParamService>();
            services.AddScoped<IPermissionService, PermissionService>();
        }

        /// <summary>
        /// ��ʼ�����ݿ�
        /// </summary>
        private static async Task InitializeDatabaseAsync()
        {
            var dbService = _serviceProvider.GetRequiredService<SysDbContextService>();

            // ȷ�����ݿⴴ��
            await dbService.EnsureDatabaseCreatedAsync();

            // ��ʼ����������
            await dbService.InitializeDatabaseAsync();
        }
    }
}