using MediatR;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Commands.Products;

/// <summary>
/// Command to update product stock
/// </summary>
public record UpdateProductStockCommand(
    Guid ProductId,
    int NewQuantity,
    string Reason
) : IRequest<Result<Unit>>;