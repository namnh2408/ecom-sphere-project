using MediatR;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Commands.Products;

/// <summary>
/// Command to deactivate a product
/// </summary>
public record DeactivateProductCommand(
    Guid ProductId
) : IRequest<Result<Unit>>;