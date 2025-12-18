namespace Catalog.Application.DTOs;

/// <summary>
/// Data Transfer Object for Product
/// </summary>
public record ProductDto(
    Guid Id,
    string Name,
    string Sku,
    string? Description,
    decimal Price,
    decimal? CostPrice,
    int StockQuantity,
    bool IsStockLow,
    Guid CategoryId,
    string Status,
    string? PrimaryImagePath,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

/// <summary>
/// Paginated result for products
/// </summary>
public record PaginatedProductResult(
    IEnumerable<ProductDto> Products,
    int TotalCount,
    int PageNumber,
    int PageSize
);