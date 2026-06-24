using System.Collections.ObjectModel;
using WinformTemplate.Business.Demo.Model;
using WinformTemplate.Business.Demo.Repositories;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Common.MVVM;
using WinformTemplate.Common.MVVM.Command;
using WinformTemplate.Logger;

namespace WinformTemplate.Business.Demo.ViewModel;

public sealed class DemoNoteManagementViewModel : BaseViewModel
{
    private readonly IDemoNoteRepository _repository;
    private ObservableCollection<DemoNote> _notes = new();
    private DemoNote? _selectedNote;
    private string? _searchKeyword;
    private int _currentPage = 1;
    private int _pageSize = 20;
    private int _totalCount;
    private string _sortBy = "CreateAt";
    private bool _sortAscending;

    public DemoNoteManagementViewModel(IDemoNoteRepository repository, string dataSourceName)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        DataSourceName = string.IsNullOrWhiteSpace(dataSourceName) ? "Unknown" : dataSourceName;

        LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
        SearchCommand = new RelayCommand(async () => await SearchAsync());
        ResetSearchCommand = new RelayCommand(async () => await ResetSearchAsync());
        RefreshCommand = new RelayCommand(async () => await LoadDataAsync());
        PreviousPageCommand = new RelayCommand(async () => await GoToPageAsync(CurrentPage - 1), () => HasPreviousPage);
        NextPageCommand = new RelayCommand(async () => await GoToPageAsync(CurrentPage + 1), () => HasNextPage);
    }

    public string DataSourceName { get; }

    public string DataSourceLabel => $"当前数据源：{DataSourceName}";

    public ObservableCollection<DemoNote> Notes
    {
        get => _notes;
        private set => SetProperty(ref _notes, value);
    }

    public DemoNote? SelectedNote
    {
        get => _selectedNote;
        set => SetProperty(ref _selectedNote, value);
    }

    public string? SearchKeyword
    {
        get => _searchKeyword;
        set => SetProperty(ref _searchKeyword, value);
    }

    public int CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (SetProperty(ref _currentPage, Math.Max(value, 1)))
            {
                RaisePagingPropertiesChanged();
            }
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (SetProperty(ref _pageSize, Math.Max(value, 1)))
            {
                CurrentPage = 1;
                RaisePagingPropertiesChanged();
            }
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        private set
        {
            if (SetProperty(ref _totalCount, Math.Max(value, 0)))
            {
                RaisePagingPropertiesChanged();
            }
        }
    }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;

    public string SortBy
    {
        get => _sortBy;
        set => SetProperty(ref _sortBy, string.IsNullOrWhiteSpace(value) ? "CreateAt" : value);
    }

    public bool SortAscending
    {
        get => _sortAscending;
        set => SetProperty(ref _sortAscending, value);
    }

    public RelayCommand LoadDataCommand { get; }

    public RelayCommand SearchCommand { get; }

    public RelayCommand ResetSearchCommand { get; }

    public RelayCommand RefreshCommand { get; }

    public RelayCommand PreviousPageCommand { get; }

    public RelayCommand NextPageCommand { get; }

    public event EventHandler<string>? OperationCompleted;

    public override async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            await LoadDataCoreAsync();
        }
        catch (Exception ex)
        {
            Debug.Error("Load DemoNote data failed", ex);
            StatusMessage = FormatExceptionMessage(ex);
            Notes = new ObservableCollection<DemoNote>();
            TotalCount = 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadDataAsync();
    }

    public async Task ResetSearchAsync()
    {
        SearchKeyword = null;
        SortBy = "CreateAt";
        SortAscending = false;
        CurrentPage = 1;
        await LoadDataAsync();
    }

    public async Task GoToPageAsync(int page)
    {
        CurrentPage = Math.Clamp(page, 1, TotalPages);
        await LoadDataAsync();
    }

    public async Task ApplySortAsync(string sortBy, bool ascending)
    {
        SortBy = sortBy;
        SortAscending = ascending;
        CurrentPage = 1;
        await LoadDataAsync();
    }

    public async Task<(bool Success, string Message)> SaveNoteAsync(DemoNote note)
    {
        ArgumentNullException.ThrowIfNull(note);

        try
        {
            Validate(note);
            IsBusy = true;

            if (note.Id > 0)
            {
                var updated = await _repository.UpdateAsync(note);
                if (!updated)
                {
                    return Fail("保存失败：便签不存在。");
                }
            }
            else
            {
                await _repository.AddAsync(note);
            }

            await LoadDataCoreAsync();
            return Succeed("保存成功。");
        }
        catch (Exception ex)
        {
            Debug.Error("Save DemoNote failed", ex);
            return Fail(FormatExceptionMessage(ex));
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<(bool Success, string Message)> DeleteNoteAsync(DemoNote? note)
    {
        if (note == null)
        {
            return Fail("请先选择便签。");
        }

        try
        {
            IsBusy = true;
            var deleted = await _repository.DeleteAsync(note.Id);
            if (!deleted)
            {
                return Fail("删除失败：便签不存在。");
            }

            SelectedNote = null;
            await LoadDataCoreAsync();
            return Succeed("删除成功。");
        }
        catch (Exception ex)
        {
            Debug.Error("Delete DemoNote failed", ex);
            return Fail(FormatExceptionMessage(ex));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadDataCoreAsync()
    {
        var result = await _repository.SearchByTitleAsync(
            SearchKeyword,
            CurrentPage,
            PageSize,
            SortBy,
            SortAscending);

        Notes = new ObservableCollection<DemoNote>(result.Items);
        TotalCount = result.Total;
        StatusMessage = string.Empty;
    }

    private (bool Success, string Message) Succeed(string message)
    {
        StatusMessage = message;
        OperationCompleted?.Invoke(this, message);
        return (true, message);
    }

    private (bool Success, string Message) Fail(string message)
    {
        StatusMessage = message;
        return (false, message);
    }

    private void RaisePagingPropertiesChanged()
    {
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(HasPreviousPage));
        OnPropertyChanged(nameof(HasNextPage));
    }

    private static void Validate(DemoNote note)
    {
        if (string.IsNullOrWhiteSpace(note.Title))
        {
            throw new InvalidOperationException("标题不能为空。");
        }

        if (note.Title.Length > 120)
        {
            throw new InvalidOperationException("标题不能超过 120 个字符。");
        }

        if (note.Content.Length > 4000)
        {
            throw new InvalidOperationException("内容不能超过 4000 个字符。");
        }
    }

    private static string FormatExceptionMessage(Exception ex)
    {
        return ex is DataSourceUnavailableException
            ? "未连接后端，请先启动 WinformTemplateServer。"
            : ex.Message;
    }
}
