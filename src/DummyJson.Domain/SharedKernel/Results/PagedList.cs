namespace SharedKernel.Results;

/// <summary>
/// A standardized wrapper for paginated data.
/// </summary>
public sealed record PagedList<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int CurrentCount => Items.Count;
    public int Total => TotalCount;
    public int Skip => (Page - 1) * PageSize;
    public int Limit => PageSize;

    public bool HasNextPage => Page * PageSize < TotalCount;
    public bool HasPreviousPage => Page > 1;
}
