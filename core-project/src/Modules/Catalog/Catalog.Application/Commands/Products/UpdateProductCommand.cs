using MediatR;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Commands.Products;

/// <summary>
/// Command to update a product
/// </summary>
public record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string? Description,
    decimal Price,
    decimal? CostPrice,
    Guid CategoryId,
    string? PrimaryImagePath
) : IRequest<Result<Unit>>;