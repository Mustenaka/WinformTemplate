using Microsoft.Extensions.DependencyInjection;

namespace WinformTemplate.Common.DataAccess;

public static class ModuleRepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddDataSourceOptions(this IServiceCollection services, DataSourceOptions options)
    {
        return services.AddSingleton(options);
    }

    public static IServiceCollection AddModuleRepository<TInterface, TEfImplementation, TApiImplementation, TLocalImplementation>(
        this IServiceCollection services,
        string moduleKey)
        where TInterface : class
        where TEfImplementation : class, TInterface
        where TApiImplementation : class, TInterface
        where TLocalImplementation : class, TInterface
    {
        services.AddScoped<TEfImplementation>();
        services.AddScoped<TApiImplementation>();
        services.AddScoped<TLocalImplementation>();

        services.AddScoped<TInterface>(sp =>
        {
            var options = sp.GetRequiredService<DataSourceOptions>();
            return options.Resolve(moduleKey) switch
            {
                DataSourceKind.Ef => sp.GetRequiredService<TEfImplementation>(),
                DataSourceKind.WebApi => sp.GetRequiredService<TApiImplementation>(),
                DataSourceKind.Local => sp.GetRequiredService<TLocalImplementation>(),
                _ => throw new InvalidOperationException($"Module {moduleKey} has an invalid data source configuration.")
            };
        });

        return services;
    }
}
