using MediatR;
using Catalog.Application.DTOs;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Queries.Products;

/// <summary>
/// Query to get all products (paginated)
/// </summary>
public record GetAllProductsQuery(
    int PageNumber,
    int PageSize
) : IRequest<Result<PaginatedProductResult>>;