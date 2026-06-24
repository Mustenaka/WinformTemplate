namespace WinformTemplate.Common.DataAccess;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    public int Total { get; set; }
}
