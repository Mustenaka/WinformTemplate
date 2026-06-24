using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Serialize;

namespace WinformTemplate.Business.Demo.Context;

public static class DemoDbContextService
{
    public static IServiceCollection AddDemoDatabase(
        IServiceCollection services,
        bool enableDetailedErrors = false,
        bool enableSensitiveDataLogging = false)
    {
        var config = GlobalProjectConfig.Instance.Config;

        services.AddDbContext<DemoDbContext>(options =>
        {
            EfDbContextOptions.UseConfiguredDatabase(options, config?.Ef, "Demo");

            if (enableDetailedErrors)
            {
                options.EnableDetailedErrors();
            }

            if (enableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddScoped<IDatabaseInitializer, DemoDatabaseInitializer>();

        return services;
    }
}
