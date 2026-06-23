using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Serialize;

namespace WinformTemplate.Business.Template.Context;

public static class TemplateDbContextService
{
    public static void AddTemplateDatabase(IServiceCollection services, bool isDevelopment, bool enableSensitiveDataLogging)
    {
        var config = GlobalProjectConfig.Instance.Config;

        services.AddDbContext<TemplateDbContext>(options =>
        {
            EfDbContextOptions.UseConfiguredDatabase(options, config?.Ef, "Template");

            if (isDevelopment)
            {
                options.EnableDetailedErrors();
            }

            if (enableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }
        }, ServiceLifetime.Scoped);

        services.AddScoped<IDatabaseInitializer, TemplateDatabaseInitializer>();
    }
}
