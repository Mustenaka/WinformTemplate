using NUnit.Framework;
using WinformTemplate.Business.Demo.Model;
using WinformTemplate.Business.Demo.Repositories;
using WinformTemplate.Business.Demo.ViewModel;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Tests.Business.Demo;

public sealed class DemoNoteManagementViewModelTests
{
    [Test]
    public async Task LoadDataAsync_UsesPagingSearchAndSortState()
    {
        var repository = new FakeDemoNoteRepository();
        var viewModel = new DemoNoteManagementViewModel(repository, "Local");

        viewModel.PageSize = 1;
        viewModel.SearchKeyword = "Alpha";
        await viewModel.ApplySortAsync("Title", ascending: true);

        Assert.That(repository.LastSearch?.TitleKeyword, Is.EqualTo("Alpha"));
        Assert.That(repository.LastSearch?.PageIndex, Is.EqualTo(1));
        Assert.That(repository.LastSearch?.PageSize, Is.EqualTo(1));
        Assert.That(repository.LastSearch?.OrderBy, Is.EqualTo("Title"));
        Assert.That(repository.LastSearch?.Ascending, Is.True);
        Assert.That(viewModel.TotalCount, Is.EqualTo(2));
        Assert.That(viewModel.Notes.Single().Title, Is.EqualTo("Alpha note"));
    }

    [Test]
    public async Task SaveAndDeleteNoteAsync_PerformCrudAndRefresh()
    {
        var repository = new FakeDemoNoteRepository();
        var viewModel = new DemoNoteManagementViewModel(repository, "EF");

        await viewModel.LoadDataAsync();
        var save = await viewModel.SaveNoteAsync(new DemoNote
        {
            Title = "Created note",
            Content = "Created by test"
        });

        Assert.That(save.Success, Is.True);
        Assert.That(repository.AddCount, Is.EqualTo(1));
        Assert.That(viewModel.TotalCount, Is.EqualTo(4));

        var created = viewModel.Notes.Single(note => note.Title == "Created note");
        created.Content = "Updated";
        var update = await viewModel.SaveNoteAsync(created);
        Assert.That(update.Success, Is.True);
        Assert.That(repository.UpdateCount, Is.EqualTo(1));

        var delete = await viewModel.DeleteNoteAsync(created);
        Assert.That(delete.Success, Is.True);
        Assert.That(repository.DeleteCount, Is.EqualTo(1));
        Assert.That(viewModel.Notes.Any(note => note.Id == created.Id), Is.False);
    }

    [Test]
    public async Task LoadDataAsync_WhenBackendUnavailable_ShowsDisconnectedStatus()
    {
        var repository = new ThrowingDemoNoteRepository();
        var viewModel = new DemoNoteManagementViewModel(repository, "WebAPI");

        await viewModel.LoadDataAsync();

        Assert.That(viewModel.StatusMessage, Does.Contain("未连接后端"));
        Assert.That(viewModel.Notes, Is.Empty);
    }

    private sealed class FakeDemoNoteRepository : IDemoNoteRepository
    {
        private readonly List<DemoNote> _notes = new()
        {
            new DemoNote { Id = 1, Title = "Alpha note", Content = "First", Pinned = true, CreateAt = DateTime.Today.AddHours(1), UpdateAt = DateTime.Today.AddHours(1) },
            new DemoNote { Id = 2, Title = "Beta note", Content = "Second", Pinned = false, CreateAt = DateTime.Today.AddHours(2), UpdateAt = DateTime.Today.AddHours(2) },
            new DemoNote { Id = 3, Title = "Alpha second", Content = "Third", Pinned = false, CreateAt = DateTime.Today.AddHours(3), UpdateAt = DateTime.Today.AddHours(3) }
        };

        public SearchCall? LastSearch { get; private set; }

        public int AddCount { get; private set; }

        public int UpdateCount { get; private set; }

        public int DeleteCount { get; private set; }

        public Task<DemoNote?> GetByIdAsync(object id)
        {
            var noteId = Convert.ToInt64(id);
            return Task.FromResult(_notes.FirstOrDefault(note => note.Id == noteId));
        }

        public Task<PagedResult<DemoNote>> QueryAsync(QueryRequest req)
        {
            return SearchByTitleAsync(req.Keyword, req.Page, req.PageSize, req.SortBy, !req.Desc);
        }

        public Task<DemoNote> AddAsync(DemoNote entity)
        {
            AddCount++;
            entity.Id = _notes.Max(note => note.Id) + 1;
            entity.CreateAt = DateTime.Now;
            entity.UpdateAt = DateTime.Now;
            _notes.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<bool> UpdateAsync(DemoNote entity)
        {
            UpdateCount++;
            var index = _notes.FindIndex(note => note.Id == entity.Id);
            if (index < 0)
            {
                return Task.FromResult(false);
            }

            _notes[index] = entity;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(object id)
        {
            DeleteCount++;
            var noteId = Convert.ToInt64(id);
            return Task.FromResult(_notes.RemoveAll(note => note.Id == noteId) > 0);
        }

        public Task<PagedResult<DemoNote>> SearchByTitleAsync(
            string? titleKeyword = null,
            int pageIndex = 1,
            int pageSize = 20,
            string? orderBy = null,
            bool ascending = false)
        {
            LastSearch = new SearchCall(titleKeyword, pageIndex, pageSize, orderBy, ascending);

            IEnumerable<DemoNote> query = _notes;
            if (!string.IsNullOrWhiteSpace(titleKeyword))
            {
                query = query.Where(note => note.Title.Contains(titleKeyword, StringComparison.OrdinalIgnoreCase));
            }

            var total = query.Count();
            query = orderBy switch
            {
                "Title" => ascending ? query.OrderBy(note => note.Title) : query.OrderByDescending(note => note.Title),
                _ => query.OrderByDescending(note => note.CreateAt)
            };

            var items = query
                .Skip((Math.Max(pageIndex, 1) - 1) * Math.Max(pageSize, 1))
                .Take(Math.Max(pageSize, 1))
                .ToList();

            return Task.FromResult(new PagedResult<DemoNote>
            {
                Items = items,
                Total = total
            });
        }
    }

    private sealed class ThrowingDemoNoteRepository : IDemoNoteRepository
    {
        public Task<DemoNote?> GetByIdAsync(object id) => throw CreateUnavailable();

        public Task<PagedResult<DemoNote>> QueryAsync(QueryRequest req) => throw CreateUnavailable();

        public Task<DemoNote> AddAsync(DemoNote entity) => throw CreateUnavailable();

        public Task<bool> UpdateAsync(DemoNote entity) => throw CreateUnavailable();

        public Task<bool> DeleteAsync(object id) => throw CreateUnavailable();

        public Task<PagedResult<DemoNote>> SearchByTitleAsync(
            string? titleKeyword = null,
            int pageIndex = 1,
            int pageSize = 20,
            string? orderBy = null,
            bool ascending = false) => throw CreateUnavailable();

        private static DataSourceUnavailableException CreateUnavailable()
        {
            return new DataSourceUnavailableException("Demo", "/api/Demo/notes", new InvalidOperationException("offline"));
        }
    }

    public sealed record SearchCall(string? TitleKeyword, int PageIndex, int PageSize, string? OrderBy, bool Ascending);
}
