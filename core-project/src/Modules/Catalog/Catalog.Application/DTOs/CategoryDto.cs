namespace Catalog.Application.DTOs;

/// <summary>
/// Data Transfer Object for Category
/// </summary>
public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

/// <summary>
/// Category DTO with product count
/// </summary>
public record CategoryWithCountDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    bool IsActive,
    int ProductCount,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

/// <summary>
/// Paginated result for categories
/// </summary>
public record PaginatedCategoryResult(
    IEnumerable<CategoryDto> Categories,
    int TotalCount,
    int PageNumber,
    int PageSize
);