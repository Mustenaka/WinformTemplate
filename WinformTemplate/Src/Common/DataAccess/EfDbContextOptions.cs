using Microsoft.EntityFrameworkCore;
using WinformTemplate.Common.Config;

namespace WinformTemplate.Common.DataAccess;

public static class EfDbContextOptions
{
    public static void UseConfiguredDatabase(DbContextOptionsBuilder optionsBuilder, EfConfig? config, string? moduleKey = null)
    {
        var efConfig = config ?? new EfConfig();
        var dbType = efConfig.DbType.ToLowerInvariant();

        if (dbType == "sqlite")
        {
            var fullPath = ResolveSqlitePath(efConfig.SQLitePath, moduleKey);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            optionsBuilder.UseSqlite($"Data Source={fullPath}");
            return;
        }

        if (dbType == "mysql")
        {
            if (string.IsNullOrWhiteSpace(efConfig.MySqlConnection))
            {
                throw new InvalidOperationException("MySQL connection string is not configured.");
            }

            optionsBuilder.UseMySql(efConfig.MySqlConnection, new MySqlServerVersion(new Version(8, 0, 21)));
            return;
        }

        throw new InvalidOperationException($"Unsupported database type: {efConfig.DbType}");
    }

    public static string ResolveSqlitePath(string configuredPath, string? moduleKey = null)
    {
        var fullConfiguredPath = Path.GetFullPath(configuredPath);
        if (string.IsNullOrWhiteSpace(moduleKey))
        {
            return fullConfiguredPath;
        }

        var directory = Path.HasExtension(fullConfiguredPath)
            ? Path.GetDirectoryName(fullConfiguredPath) ?? Directory.GetCurrentDirectory()
            : fullConfiguredPath;
        var fileName = $"{moduleKey.Trim().ToLowerInvariant()}.db";
        return Path.GetFullPath(Path.Combine(directory, fileName));
    }
}
