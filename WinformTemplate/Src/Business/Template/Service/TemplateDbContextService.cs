using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WinformTemplate.Business.Template.Context;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Logger;
using WinformTemplate.Serialize;

namespace WinformTemplate.Business.Template.Service;

/// <summary>
/// 模板数据库上下文服务
/// 负责数据库的创建和初始化
/// </summary>
public class TemplateDbContextService
{
    private readonly TemplateDbContext _dbContext;

    public TemplateDbContextService(TemplateDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// 添加模板数据库服务到依赖注入容器
    /// </summary>
    public static void AddTemplateDatabase(IServiceCollection services, bool isDevelopment, bool enableSensitiveDataLogging)
    {
        var config = GlobalProjectConfig.Instance.Config;
        var dbType = config?.DbType?.ToLower() ?? "sqlite";

        services.AddDbContext<TemplateDbContext>((serviceProvider, options) =>
        {
            if (dbType == "sqlite")
            {
                var sqlitePath = config?.SQLiteDB ?? ".\\Resources\\Database\\sys.db";
                var fullPath = Path.GetFullPath(sqlitePath);

                // 确保目录存在
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                options.UseSqlite($"Data Source={fullPath}");
            }
            else if (dbType == "mysql")
            {
                var connectionString = config?.DB;
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("MySQL 连接字符串未配置");
                }

                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)));
            }

            // 开发环境配置
            if (isDevelopment)
            {
                options.EnableDetailedErrors();
            }

            if (enableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }
        }, ServiceLifetime.Scoped);

        // 注册服务
        services.AddScoped<TemplateDbContextService>();
    }

    /// <summary>
    /// 确保数据库已创建
    /// </summary>
    public async Task EnsureDatabaseCreatedAsync()
    {
        try
        {
            await _dbContext.Database.EnsureCreatedAsync();
            Debug.Info("模板数据库已创建或已存在");
        }
        catch (Exception ex)
        {
            Debug.Error("创建模板数据库失败", ex);
            throw;
        }
    }

    /// <summary>
    /// 初始化数据库（插入种子数据）
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        try
        {
            // 检查表是否存在（通过尝试查询）
            bool tablesExist = true;
            try
            {
                var hasCategories = await _dbContext.Categories.AnyAsync();
                var hasProducts = await _dbContext.Products.AnyAsync();

                if (hasCategories && hasProducts)
                {
                    Debug.Info("模板数据库已包含数据，跳过初始化");
                    return;
                }

                // 表存在但没有数据，继续初始化
            }
            catch (Exception)
            {
                // 表不存在，需要创建
                tablesExist = false;
                Debug.Info("模板数据表不存在，将重新创建");
            }

            // 如果表不存在，先创建数据库
            if (!tablesExist)
            {
                await _dbContext.Database.EnsureCreatedAsync();
                Debug.Info("模板数据表已创建");
            }

            Debug.Info("开始初始化模板数据库种子数据...");

            // 初始化分类数据
            await InitializeCategoriesAsync();

            // 初始化产品数据
            await InitializeProductsAsync();

            Debug.Info("模板数据库初始化完成");
        }
        catch (Exception ex)
        {
            Debug.Error("初始化模板数据库失败", ex);
            throw;
        }
    }

    /// <summary>
    /// 初始化分类数据
    /// </summary>
    private async Task InitializeCategoriesAsync()
    {
        var categories = new List<CategoryModel>
        {
            new CategoryModel
            {
                Name = "电子产品",
                ParentId = null,
                Level = 0,
                SortOrder = 1,
                Status = 0,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            },
            new CategoryModel
            {
                Name = "服装鞋帽",
                ParentId = null,
                Level = 0,
                SortOrder = 2,
                Status = 0,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            },
            new CategoryModel
            {
                Name = "食品饮料",
                ParentId = null,
                Level = 0,
                SortOrder = 3,
                Status = 0,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            },
            new CategoryModel
            {
                Name = "图书文具",
                ParentId = null,
                Level = 0,
                SortOrder = 4,
                Status = 0,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            },
            new CategoryModel
            {
                Name = "运动户外",
                ParentId = null,
                Level = 0,
                SortOrder = 5,
                Status = 0,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            }
        };

        await _dbContext.Categories.AddRangeAsync(categories);
        await _dbContext.SaveChangesAsync();

        Debug.Info($"已添加 {categories.Count} 个分类");
    }

    /// <summary>
    /// 初始化产品数据
    /// </summary>
    private async Task InitializeProductsAsync()
    {
        // 获取分类
        var categories = await _dbContext.Categories.ToListAsync();
        if (!categories.Any())
        {
            Debug.Warn("没有找到分类数据，无法初始化产品");
            return;
        }

        var random = new Random();
        var products = new List<ProductModel>();

        // 为每个分类生成产品
        foreach (var category in categories)
        {
            var categoryName = string.IsNullOrWhiteSpace(category.Name) ? "分类" : category.Name;
            for (int i = 1; i <= 20; i++)
            {
                var product = new ProductModel
                {
                    Name = $"{categoryName} - 产品 {i}",
                    Code = $"{categoryName[0]}{category.Id:D2}{i:D3}",
                    CategoryId = category.Id,
                    Price = Math.Round((decimal)(random.NextDouble() * 1000 + 10), 2),
                    Stock = random.Next(0, 500),
                    Status = random.Next(0, 3), // 0-正常, 1-停用, 2-缺货
                    Description = $"这是{categoryName}的第{i}个示例产品，用于展示产品管理功能。",
                    Tags = $"示例,{categoryName},测试数据",
                    CreateAt = DateTime.Now.AddDays(-random.Next(0, 365)),
                    UpdateAt = DateTime.Now
                };

                products.Add(product);
            }
        }

        await _dbContext.Products.AddRangeAsync(products);
        await _dbContext.SaveChangesAsync();

        Debug.Info($"已添加 {products.Count} 个产品");
    }

    /// <summary>
    /// 清空所有数据（谨慎使用）
    /// </summary>
    public async Task ClearAllDataAsync()
    {
        Debug.Warn("开始清空模板数据库所有数据...");

        _dbContext.Products.RemoveRange(_dbContext.Products);
        _dbContext.Categories.RemoveRange(_dbContext.Categories);
        _dbContext.ImportRecords.RemoveRange(_dbContext.ImportRecords);

        await _dbContext.SaveChangesAsync();

        Debug.Info("模板数据库所有数据已清空");
    }

    /// <summary>
    /// 重置数据库（删除并重新创建）
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        Debug.Warn("开始重置模板数据库...");

        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();
        await InitializeDatabaseAsync();

        Debug.Info("模板数据库已重置");
    }
}
