using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using WinformTemplate.Bootstrap;
using WinformTemplate.Common.Config;
using WinformTemplate.Navigation;

namespace WinformTemplate.Tests.Navigation;

[Apartment(ApartmentState.STA)]
public sealed class PageConstructionSmokeTests
{
    private string? _tempDirectory;

    [SetUp]
    public void SetUp()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "WinformTemplate.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        SqliteConnection.ClearAllPools();

        if (!string.IsNullOrWhiteSpace(_tempDirectory) && Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Test]
    public void DefaultRegisteredPages_ConstructWithRealServiceProvider()
    {
        var config = CreateEfConfig(Path.Combine(_tempDirectory!, "Resources", "Database", "app.db"));
        var serviceProvider = AppServiceRegistration.BuildServiceProvider(config, isDevelopment: false);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var registry = scope.ServiceProvider.GetRequiredService<IPageRegistry>();
            var menuKeys = registry.MenuKeys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase).ToArray();

            Assert.That(menuKeys, Is.EquivalentTo(new[]
            {
                "/sys/user",
                "/sys/role",
                "/template/product",
                "/demo/note-ef",
                "/demo/note-api",
                "/demo/note-local"
            }));

            foreach (var menuKey in menuKeys)
            {
                UserControl? page = null;
                Assert.DoesNotThrow(() =>
                {
                    var resolved = registry.TryResolve(menuKey, scope.ServiceProvider, out page);
                    Assert.That(resolved, Is.True, $"Menu key '{menuKey}' should resolve.");
                    Assert.That(page, Is.Not.Null, $"Menu key '{menuKey}' should construct a page.");
                }, $"Menu key '{menuKey}' should construct without depending on a rendered size.");

                page?.Dispose();
            }
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
