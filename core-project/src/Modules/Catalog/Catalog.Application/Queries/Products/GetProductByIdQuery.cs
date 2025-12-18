using MediatR;
using Catalog.Application.DTOs;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Queries.Products;

/// <summary>
/// Query to get a product by ID
/// </summary>
public record GetProductByIdQuery(
    Guid ProductId
) : IRequest<Result<ProductDto>>;