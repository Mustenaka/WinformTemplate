using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using WinformTemplate.Bootstrap;
using WinformTemplate.Business.Demo.Repositories;
using WinformTemplate.Business.Sys.Service;
using WinformTemplate.Business.Template.Service.Interface;
using WinformTemplate.Common.Config;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Serialize;

namespace WinformTemplate.Tests.Startup;

public sealed class AppStartupIntegrationTests
{
    private ProjectConfig? _previousConfig;
    private string? _tempDirectory;

    [SetUp]
    public void SetUp()
    {
        _previousConfig = GlobalProjectConfig.Instance.Config;
        _tempDirectory = Path.Combine(Path.GetTempPath(), "WinformTemplate.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        GlobalProjectConfig.Instance.Config = _previousConfig;
        SqliteConnection.ClearAllPools();

        if (!string.IsNullOrWhiteSpace(_tempDirectory) && Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Test]
    public async Task EfStartupSequence_InitializesSysTemplateAndDemoThenSupportsQueries()
    {
        var config = CreateEfConfig(Path.Combine(_tempDirectory!, "Resources", "Database", "app.db"));
        var serviceProvider = AppServiceRegistration.BuildServiceProvider(config, isDevelopment: false);

        try
        {
            await AppServiceRegistration.InitializeDatabaseAsync(serviceProvider);

            Assert.That(File.Exists(EfDbContextOptions.ResolveSqlitePath(config.Ef.SQLitePath, "Sys")), Is.True);
            Assert.That(File.Exists(EfDbContextOptions.ResolveSqlitePath(config.Ef.SQLitePath, "Template")), Is.True);
            Assert.That(File.Exists(EfDbContextOptions.ResolveSqlitePath(config.Ef.SQLitePath, "Demo")), Is.True);

            using var scope = serviceProvider.CreateScope();
            var accountService = scope.ServiceProvider.GetRequiredService<ISysAccountService>();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
            var demoRepository = scope.ServiceProvider.GetRequiredService<EfDemoNoteRepository>();

            var admin = await accountService.LoginAsync("admin", "123456");
            Assert.That(admin, Is.Not.Null);

            var (items, totalCount) = await productService.SearchProductsAsync(
                pageIndex: 1,
                pageSize: 5,
                orderBy: "CreateAt",
                ascending: false);
            Assert.That(totalCount, Is.GreaterThan(0));
            Assert.That(items, Is.Not.Empty);
            Assert.That(items.Count, Is.LessThanOrEqualTo(5));

            var demoPage = await demoRepository.SearchByTitleAsync(pageIndex: 1, pageSize: 5);
            Assert.That(demoPage.Total, Is.GreaterThan(0));
            Assert.That(demoPage.Items, Is.Not.Empty);
        }
        finally
        {
            (serviceProvider as IDisposable)?.Dispose();
        }
    }

    private static ProjectConfig CreateEfConfig(string sqlitePath)
    {
        return new ProjectConfig
        {
            DataSource = new DataSourceConfig
            {
                Default = "Ef",
                Modules = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Sys"] = "Ef",
                    ["Template"] = "Ef",
                    ["Demo"] = "Ef"
                }
            },
            Ef = new EfConfig
            {
                DbType = "SQLite",
                SQLitePath = sqlitePath,
                MySqlConnection = "server=127.0.0.1;port=3306;user=root;database=base;password=__SET_ME__;"
            },
            WebApi = new WebApiConfig(),
            Local = new LocalConfig(),
            Security = new SecurityConfig()
        };
    }
}
