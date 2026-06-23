using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using WinformTemplate.Business.Template.Context;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Business.Template.Repositories;

namespace WinformTemplate.Tests.Business.Template;

public sealed class TemplateRepositoryDataSourceTests
{
    private string? _tempDirectory;

    [SetUp]
    public void Setup()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "WinformTemplate.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (!string.IsNullOrWhiteSpace(_tempDirectory) && Directory.Exists(_tempDirectory))
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Test]
    public async Task ProductRepositoryContract_EfAndLocal_ReturnConsistentBehavior()
    {
        var dbPath = Path.Combine(_tempDirectory!, "template.db");
        var options = new DbContextOptionsBuilder<TemplateDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        await using var context = new TemplateDbContext(options);
        await context.Database.EnsureCreatedAsync();
        context.Categories.AddRange(CreateCategories());
        context.Products.AddRange(CreateProducts());
        await context.SaveChangesAsync();

        var efCategoryRepository = new EfCategoryRepository(context);
        var efProductRepository = new EfProductRepository(context);

        var seedRoot = Path.Combine(_tempDirectory!, "mock");
        Directory.CreateDirectory(seedRoot);
        WriteJson(Path.Combine(seedRoot, "categories.json"), CreateCategories());
        WriteJson(Path.Combine(seedRoot, "products.json"), CreateProducts());
        var localCategoryRepository = new LocalCategoryRepository(seedRoot);
        var localProductRepository = new LocalProductRepository(seedRoot);

        await AssertProductContractAsync("Ef", efProductRepository, efCategoryRepository);
        await AssertProductContractAsync("Local", localProductRepository, localCategoryRepository);
    }

    private static async Task AssertProductContractAsync(
        string sourceName,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        var activeCategories = await categoryRepository.GetActiveCategoriesAsync();
        Assert.That(activeCategories.Count, Is.EqualTo(2), sourceName);

        var page = await productRepository.SearchProductsAsync(
            keyword: "Key",
            categoryId: 1,
            pageIndex: 1,
            pageSize: 10,
            orderBy: "name",
            ascending: true);
        Assert.That(page.Total, Is.EqualTo(1), sourceName);
        Assert.That(page.Items.Single().Code, Is.EqualTo("EL-KEY-001"), sourceName);
        Assert.That(page.Items.Single().Category?.Name, Is.EqualTo("Electronics"), sourceName);

        Assert.That(await productRepository.IsCodeExistsAsync("EL-KEY-001"), Is.True, sourceName);
        Assert.That(await productRepository.GetCountByCategoryAsync(1), Is.EqualTo(1), sourceName);

        var created = await productRepository.AddAsync(new ProductModel
        {
            Name = "Camera",
            Code = "EL-CAM-001",
            CategoryId = 1,
            Price = 399,
            Stock = 5,
            Status = 0
        });
        Assert.That(created.Id, Is.GreaterThan(0), sourceName);
        Assert.That(await productRepository.IsCodeExistsAsync("EL-CAM-001"), Is.True, sourceName);

        created.Status = 1;
        Assert.That(await productRepository.UpdateAsync(created), Is.True, sourceName);
        Assert.That((await productRepository.GetByIdAsync(created.Id))?.Status, Is.EqualTo(1), sourceName);

        Assert.That(await productRepository.DeleteByIdsAsync(new[] { created.Id }), Is.EqualTo(1), sourceName);
        Assert.That(await productRepository.GetByIdAsync(created.Id), Is.Null, sourceName);
    }

    private static List<CategoryModel> CreateCategories()
    {
        return new List<CategoryModel>
        {
            new CategoryModel { Id = 1, Name = "Electronics", Status = 0, SortOrder = 1, Level = 0, CreateAt = DateTime.Today },
            new CategoryModel { Id = 2, Name = "Office", Status = 0, SortOrder = 2, Level = 0, CreateAt = DateTime.Today }
        };
    }

    private static List<ProductModel> CreateProducts()
    {
        return new List<ProductModel>
        {
            new ProductModel { Id = 1, Name = "Keyboard", Code = "EL-KEY-001", CategoryId = 1, Price = 129, Stock = 10, Status = 0, CreateAt = DateTime.Today },
            new ProductModel { Id = 2, Name = "Notebook", Code = "OF-NOTE-001", CategoryId = 2, Price = 9, Stock = 20, Status = 0, CreateAt = DateTime.Today }
        };
    }

    private static void WriteJson<T>(string path, T value)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }));
    }
}
