using MediatR;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Commands.Products;

/// <summary>
/// Command to delete a product (soft delete)
/// </summary>
public record DeleteProductCommand(
    Guid ProductId
) : IRequest<Result<Unit>>;