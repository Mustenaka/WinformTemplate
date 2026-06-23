namespace WinformTemplate.Common.DataAccess;

public sealed class QueryRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Keyword { get; set; }

    public string? SortBy { get; set; }

    public bool Desc { get; set; }

    public Dictionary<string, string>? Filters { get; set; }
}
