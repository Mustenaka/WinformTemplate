using Microsoft.EntityFrameworkCore;
using WinformTemplate.Common.Config;

namespace WinformTemplate.Common.DataAccess;

public static class EfDbContextOptions
{
    public static void UseConfiguredDatabase(DbContextOptionsBuilder optionsBuilder, EfConfig? config)
    {
        var efConfig = config ?? new EfConfig();
        var dbType = efConfig.DbType.ToLowerInvariant();

        if (dbType == "sqlite")
        {
            var fullPath = Path.GetFullPath(efConfig.SQLitePath);
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
                throw new InvalidOperationException("MySQL 连接字符串未配置");
            }

            optionsBuilder.UseMySql(efConfig.MySqlConnection, new MySqlServerVersion(new Version(8, 0, 21)));
            return;
        }

        throw new InvalidOperationException($"不支持的数据库类型: {efConfig.DbType}");
    }
}
