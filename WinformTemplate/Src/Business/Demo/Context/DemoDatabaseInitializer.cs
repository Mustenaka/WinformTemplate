using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Demo.Model;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Logger;

namespace WinformTemplate.Business.Demo.Context;

public sealed class DemoDatabaseInitializer : IDatabaseInitializer
{
    private readonly DemoDbContext _dbContext;

    public DemoDatabaseInitializer(DemoDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public string ModuleKey => "Demo";

    public DataSourceKind DataSourceKind => DataSourceKind.Ef;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await _dbContext.DemoNotes.AnyAsync(cancellationToken))
        {
            Debug.Info("Demo database already contains notes; skipping seed.");
            return;
        }

        var now = DateTime.Now;
        var notes = new[]
        {
            new DemoNote
            {
                Title = "EF note",
                Content = "Seeded note stored in the client Demo SQLite database.",
                Pinned = true,
                CreateAt = now.AddMinutes(-30),
                UpdateAt = now.AddMinutes(-30)
            },
            new DemoNote
            {
                Title = "Searchable note",
                Content = "Use title search, paging, and sorting on this data source.",
                Pinned = false,
                CreateAt = now.AddMinutes(-20),
                UpdateAt = now.AddMinutes(-20)
            },
            new DemoNote
            {
                Title = "Independent demo.db",
                Content = "The Demo EF module uses its own SQLite file.",
                Pinned = false,
                CreateAt = now.AddMinutes(-10),
                UpdateAt = now.AddMinutes(-10)
            }
        };

        await _dbContext.DemoNotes.AddRangeAsync(notes, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        Debug.Info($"Seeded {notes.Length} Demo notes.");
    }
}
