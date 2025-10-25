using FluentValidation;
using ShopHub.Common.Pagination;
using ShopHub.Common.CQRS;
using ShopHub.Domain.CQRS;
using ShopHub.Services.ProductService.Application.DTOs;

namespace ShopHub.Services.ProductService.Application.Queries;

/// <summary>
/// Query: Tìm kiếm sản phẩm
/// </summary>
[Cacheable(durationSeconds: 300)] // Cache 5 minutes
public class SearchProductsQuery : IQuery<PaginatedList<ProductResponseDto>>
{
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";

    public SearchProductsQuery() { }

    public SearchProductsQuery(
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 10,
        string? sortBy = null,
        string? sortDirection = "asc")
    {
        SearchTerm = searchTerm;
        PageNumber = pageNumber;
        PageSize = pageSize;
        SortBy = sortBy;
        SortDirection = sortDirection;
    }
}

/// <summary>
/// Validator cho SearchProductsQuery
/// </summary>
public class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    public SearchProductsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(200).WithMessage("Search term must not exceed 200 characters")
            .When(x => x.SearchTerm != null);

        RuleFor(x => x.SortDirection)
            .Must(x => x == null || x.Equals("asc", StringComparison.OrdinalIgnoreCase) || x.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sort direction must be 'asc' or 'desc'");
    }
}