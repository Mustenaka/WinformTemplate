using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using WinformTemplate.Business.Demo.Context;
using WinformTemplate.Business.Demo.Model;
using WinformTemplate.Business.Demo.Repositories;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Tests.Business.Demo;

public sealed class DemoNoteRepositoryTests
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
    public async Task DemoNoteRepositoryContract_EfAndLocal_ReturnConsistentCrudPagingAndSearch()
    {
        var dbPath = Path.Combine(_tempDirectory!, "demo.db");
        var options = new DbContextOptionsBuilder<DemoDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        await using var context = new DemoDbContext(options);
        await context.Database.EnsureCreatedAsync();
        context.DemoNotes.AddRange(CreateNotes());
        await context.SaveChangesAsync();
        var efRepository = new EfDemoNoteRepository(context);

        var seedRoot = Path.Combine(_tempDirectory!, "mock");
        Directory.CreateDirectory(seedRoot);
        WriteJson(Path.Combine(seedRoot, "demoNotes.json"), CreateNotes());
        var localRepository = new LocalDemoNoteRepository(seedRoot);

        await AssertContractAsync("Ef", efRepository);
        await AssertContractAsync("Local", localRepository);
    }

    private static async Task AssertContractAsync(string sourceName, IDemoNoteRepository repository)
    {
        var firstPage = await repository.SearchByTitleAsync(
            titleKeyword: "Alpha",
            pageIndex: 1,
            pageSize: 1,
            orderBy: "Title",
            ascending: true);
        Assert.That(firstPage.Total, Is.EqualTo(2), sourceName);
        Assert.That(firstPage.Items, Has.Count.EqualTo(1), sourceName);
        Assert.That(firstPage.Items[0].Title, Is.EqualTo("Alpha note"), sourceName);

        var secondPage = await repository.SearchByTitleAsync(
            titleKeyword: "Alpha",
            pageIndex: 2,
            pageSize: 1,
            orderBy: "Title",
            ascending: true);
        Assert.That(secondPage.Items.Single().Title, Is.EqualTo("Alpha second"), sourceName);

        var created = await repository.AddAsync(new DemoNote
        {
            Title = "Created note",
            Content = "Created by test",
            Pinned = false
        });
        Assert.That(created.Id, Is.GreaterThan(0), sourceName);
        Assert.That(created.CreateAt, Is.Not.EqualTo(default(DateTime)), sourceName);

        created.Title = "Updated note";
        created.Pinned = true;
        Assert.That(await repository.UpdateAsync(created), Is.True, sourceName);
        Assert.That((await repository.GetByIdAsync(created.Id))?.Title, Is.EqualTo("Updated note"), sourceName);

        var queryPage = await repository.QueryAsync(new QueryRequest
        {
            Keyword = "Updated",
            Page = 1,
            PageSize = 10,
            SortBy = "CreateAt",
            Desc = true
        });
        Assert.That(queryPage.Total, Is.EqualTo(1), sourceName);

        Assert.That(await repository.DeleteAsync(created.Id), Is.True, sourceName);
        Assert.That(await repository.GetByIdAsync(created.Id), Is.Null, sourceName);
    }

    private static List<DemoNote> CreateNotes()
    {
        return new List<DemoNote>
        {
            new DemoNote { Id = 1, Title = "Alpha note", Content = "First", Pinned = true, CreateAt = DateTime.Today.AddHours(1), UpdateAt = DateTime.Today.AddHours(1) },
            new DemoNote { Id = 2, Title = "Beta note", Content = "Second", Pinned = false, CreateAt = DateTime.Today.AddHours(2), UpdateAt = DateTime.Today.AddHours(2) },
            new DemoNote { Id = 3, Title = "Alpha second", Content = "Third", Pinned = false, CreateAt = DateTime.Today.AddHours(3), UpdateAt = DateTime.Today.AddHours(3) }
        };
    }

    private static void WriteJson<T>(string path, T value)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }));
    }
}
