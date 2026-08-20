namespace concerts_gate.server.Common.Models;

/// <summary>
/// Paginated query result wrapper for large data collections.
/// </summary>
/// <typeparam name="T">Element type of the collection.</typeparam>
public class PaginatedResult<T>
{
    /// <summary>
    /// Items on the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Current page index (1-indexed).
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// Page size / number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total count of items matching the query across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indicates whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// Indicates whether a next page exists.
    /// </summary>
    public bool HasNextPage => PageIndex < TotalPages;

    /// <summary>
    /// Initializes a new instance of <see cref="PaginatedResult{T}"/>.
    /// </summary>
    /// <param name="items">List of items on the current page.</param>
    /// <param name="totalCount">Total item count across all pages.</param>
    /// <param name="pageIndex">Current page index.</param>
    /// <param name="pageSize">Page size.</param>
    public PaginatedResult(IReadOnlyList<T> items, int totalCount, int pageIndex, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}
