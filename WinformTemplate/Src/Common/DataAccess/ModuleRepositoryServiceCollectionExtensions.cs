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
                _ => throw new InvalidOperationException($"模块 {moduleKey} 的数据源配置无效")
            };
        });

        return services;
    }

    public static IServiceCollection AddModuleRepository<TInterface, TEfImplementation>(
        this IServiceCollection services,
        string moduleKey)
        where TInterface : class
        where TEfImplementation : class, TInterface
    {
        services.AddScoped<TEfImplementation>();
        services.AddScoped<TInterface>(sp =>
        {
            var options = sp.GetRequiredService<DataSourceOptions>();
            var kind = options.Resolve(moduleKey);
            if (kind == DataSourceKind.Ef)
            {
                return sp.GetRequiredService<TEfImplementation>();
            }

            throw new InvalidOperationException($"模块 {moduleKey} 配置为 {kind}，对应仓储将在 P2 实现");
        });

        return services;
    }
}
