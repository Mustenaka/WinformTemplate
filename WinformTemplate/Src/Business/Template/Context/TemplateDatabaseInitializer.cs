using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Logger;

namespace WinformTemplate.Business.Template.Context;

public sealed class TemplateDatabaseInitializer : IDatabaseInitializer
{
    private readonly TemplateDbContext _dbContext;

    public TemplateDatabaseInitializer(TemplateDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public string ModuleKey => "Template";

    public DataSourceKind DataSourceKind => DataSourceKind.Ef;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await _dbContext.Categories.AnyAsync(cancellationToken) ||
            await _dbContext.Products.AnyAsync(cancellationToken))
        {
            Debug.Info("模板数据库已包含数据，跳过初始化");
            return;
        }

        Debug.Info("开始初始化模板数据库种子数据...");

        await InitializeCategoriesAsync(cancellationToken);
        await InitializeProductsAsync(cancellationToken);

        Debug.Info("模板数据库初始化完成");
    }

    private async Task InitializeCategoriesAsync(CancellationToken cancellationToken)
    {
        var categories = new List<CategoryModel>
        {
            new CategoryModel { Name = "电子产品", ParentId = null, Level = 0, SortOrder = 1, Status = 0, CreateAt = DateTime.Now, UpdateAt = DateTime.Now },
            new CategoryModel { Name = "服装鞋帽", ParentId = null, Level = 0, SortOrder = 2, Status = 0, CreateAt = DateTime.Now, UpdateAt = DateTime.Now },
            new CategoryModel { Name = "食品饮料", ParentId = null, Level = 0, SortOrder = 3, Status = 0, CreateAt = DateTime.Now, UpdateAt = DateTime.Now },
            new CategoryModel { Name = "图书文具", ParentId = null, Level = 0, SortOrder = 4, Status = 0, CreateAt = DateTime.Now, UpdateAt = DateTime.Now },
            new CategoryModel { Name = "运动户外", ParentId = null, Level = 0, SortOrder = 5, Status = 0, CreateAt = DateTime.Now, UpdateAt = DateTime.Now }
        };

        await _dbContext.Categories.AddRangeAsync(categories, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        Debug.Info($"已添加 {categories.Count} 个分类");
    }

    private async Task InitializeProductsAsync(CancellationToken cancellationToken)
    {
        var categories = await _dbContext.Categories.ToListAsync(cancellationToken);
        if (!categories.Any())
        {
            Debug.Warn("没有找到分类数据，无法初始化产品");
            return;
        }

        var random = new Random(20260623);
        var products = new List<ProductModel>();

        foreach (var category in categories)
        {
            var categoryName = string.IsNullOrWhiteSpace(category.Name) ? "分类" : category.Name;
            for (var i = 1; i <= 20; i++)
            {
                products.Add(new ProductModel
                {
                    Name = $"{categoryName} - 产品 {i}",
                    Code = $"{categoryName[0]}{category.Id:D2}{i:D3}",
                    CategoryId = category.Id,
                    Price = Math.Round((decimal)(random.NextDouble() * 1000 + 10), 2),
                    Stock = random.Next(0, 500),
                    Status = random.Next(0, 3),
                    Description = $"这是{categoryName}的第{i}个示例产品，用于展示产品管理功能。",
                    Tags = $"示例,{categoryName},测试数据",
                    CreateAt = DateTime.Now.AddDays(-random.Next(0, 365)),
                    UpdateAt = DateTime.Now
                });
            }
        }

        await _dbContext.Products.AddRangeAsync(products, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        Debug.Info($"已添加 {products.Count} 个产品");
    }
}
