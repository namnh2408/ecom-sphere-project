using MediatR;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Commands.Products;

/// <summary>
/// Command to create a new product
/// </summary>
public record CreateProductCommand(
    string Name,
    string Sku,
    string? Description,
    decimal Price,
    decimal? CostPrice,
    int StockQuantity,
    Guid CategoryId,
    string? PrimaryImagePath
) : IRequest<Result<Guid>>;