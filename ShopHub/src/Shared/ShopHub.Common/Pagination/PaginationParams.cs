namespace ShopHub.Common.Pagination;

/// <summary>
/// Parameters cho pagination
/// </summary>
public class PaginationParams
{
    private const int MaxPageSize = 100;
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 10;

    private int _pageNumber = DefaultPageNumber;
    private int _pageSize = DefaultPageSize;

    /// <summary>
    /// Số trang (bắt đầu từ 1)
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value > 0 ? value : DefaultPageNumber;
    }

    /// <summary>
    /// Kích thước trang (tối đa 100)
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 0 ? (value > MaxPageSize ? MaxPageSize : value) : DefaultPageSize;
    }

    /// <summary>
    /// Search keyword
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Sort column name
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (asc/desc)
    /// </summary>
    public string? SortDirection { get; set; } = "asc";

    /// <summary>
    /// Lấy số lượng records bỏ qua (offset)
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;

    /// <summary>
    /// Lấy số lượng records lấy (limit)
    /// </summary>
    public int Take => PageSize;
}