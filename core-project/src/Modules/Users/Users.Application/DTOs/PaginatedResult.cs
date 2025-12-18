namespace Users.Application.DTOs;

/// <summary>
/// Generic paginated result wrapper
/// </summary>
public record PaginatedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage
)
{
    public static PaginatedResult<T> Create(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var hasPreviousPage = pageNumber > 1;
        var hasNextPage = pageNumber < totalPages;

        return new PaginatedResult<T>(
            items,
            totalCount,
            pageNumber,
            pageSize,
            totalPages,
            hasPreviousPage,
            hasNextPage
        );
    }
}