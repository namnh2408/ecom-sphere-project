using MediatR;
using Catalog.Application.DTOs;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Queries.Products;

/// <summary>
/// Query to filter products by multiple criteria
/// </summary>
public record FilterProductsQuery(
    Guid? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Status,
    bool? InStock,
    int PageNumber,
    int PageSize
) : IRequest<Result<PaginatedProductResult>>;