using FluentValidation;
using ShopHub.Common.Pagination;
using ShopHub.Common.CQRS;
using ShopHub.Domain.CQRS;
using ShopHub.Services.ProductService.Application.DTOs;

namespace ShopHub.Services.ProductService.Application.Queries;

/// <summary>
/// Query: Lấy tất cả sản phẩm (phân trang)
/// </summary>
[Cacheable(durationSeconds: 300)] // Cache 5 minutes
public class GetAllProductsQuery : IQuery<PaginatedList<ProductResponseDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
    public bool? IsActive { get; set; }

    public GetAllProductsQuery() { }

    public GetAllProductsQuery(
        int pageNumber = 1,
        int pageSize = 10,
        string? sortBy = null,
        string? sortDirection = "asc",
        bool? isActive = true)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        SortBy = sortBy;
        SortDirection = sortDirection;
        IsActive = isActive;
    }
}

/// <summary>
/// Validator cho GetAllProductsQuery
/// </summary>
public class GetAllProductsQueryValidator : AbstractValidator<GetAllProductsQuery>
{
    public GetAllProductsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

        RuleFor(x => x.SortDirection)
            .Must(x => x == null || x.Equals("asc", StringComparison.OrdinalIgnoreCase) || x.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sort direction must be 'asc' or 'desc'");
    }
}