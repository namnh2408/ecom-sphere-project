using Microsoft.EntityFrameworkCore;

namespace ShopHub.Common.Pagination;

/// <summary>
/// Danh sách được phân trang
/// </summary>
/// <typeparam name="T">Kiểu item trong danh sách</typeparam>
public class PaginatedList<T>
{
    /// <summary>
    /// Danh sách items trên trang hiện tại
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Tổng số records
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Số trang hiện tại
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Kích thước trang
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Tổng số trang
    /// </summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    /// <summary>
    /// Có trang trước không
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Có trang sau không
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Constructor
    /// </summary>
    public PaginatedList(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    /// Tạo PaginatedList từ IQueryable
    /// </summary>
    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = source.Count();

        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Tạo PaginatedList từ IEnumerable
    /// </summary>
    public static PaginatedList<T> Create(
        IEnumerable<T> source,
        int pageNumber,
        int pageSize)
    {
        var totalCount = source.Count();

        var items = source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
    }
}