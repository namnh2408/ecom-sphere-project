using MediatR;
using Catalog.Application.DTOs;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Queries.Products;

/// <summary>
/// Query to search products by name or SKU
/// </summary>
public record SearchProductsQuery(
    string SearchTerm,
    int PageNumber,
    int PageSize
) : IRequest<Result<PaginatedProductResult>>;