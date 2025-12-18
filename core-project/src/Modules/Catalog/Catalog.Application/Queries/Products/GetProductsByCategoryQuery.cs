using MediatR;
using Catalog.Application.DTOs;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Queries.Products;

/// <summary>
/// Query to get products by category
/// </summary>
public record GetProductsByCategoryQuery(
    Guid CategoryId,
    int PageNumber,
    int PageSize
) : IRequest<Result<PaginatedProductResult>>;