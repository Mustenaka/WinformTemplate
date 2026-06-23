using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Serialize;

namespace WinformTemplate.Business.Sys.Context;

/// <summary>
/// 系统数据库上下文注册。
/// </summary>
public static class SysDbContextService
{
    public static IServiceCollection AddSysDatabase(
        IServiceCollection services,
        bool enableDetailedErrors = false,
        bool enableSensitiveDataLogging = false)
    {
        var config = GlobalProjectConfig.Instance.Config;

        services.AddDbContext<SysDbContext>(options =>
        {
            EfDbContextOptions.UseConfiguredDatabase(options, config?.Ef);

            if (enableDetailedErrors)
            {
                options.EnableDetailedErrors();
            }

            if (enableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddScoped<IDatabaseInitializer, SysDatabaseInitializer>();

        return services;
    }
}
