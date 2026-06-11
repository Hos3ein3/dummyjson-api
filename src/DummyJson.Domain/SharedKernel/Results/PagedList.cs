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
// 1-based current page number
    public int CurrentPageNumber => Page;

    // max page number, at least 1 when there are items, 0 when empty
    public int MaxPageNumber =>
        PageSize <= 0
            ? 0
            : (int)Math.Ceiling(TotalCount / (double)PageSize);
    // offset-based view, consistent with EF Skip/Take
    public int Skip => (Page - 1) * PageSize;
    public int Limit => PageSize;

    public bool HasNextPage => Page * PageSize < TotalCount;
    public bool HasPreviousPage => Page > 1;
    public int? NextPageNumber => HasNextPage ? Page + 1 : null;
    public int? PreviousPageNumber => HasPreviousPage ? Page - 1 : null;

    // “Showing X–Y of Z” helpers for UI
    public int FirstItemNumber => TotalCount == 0 ? 0 : Skip + 1;
    public int LastItemNumber  => Skip + CurrentCount;
}
