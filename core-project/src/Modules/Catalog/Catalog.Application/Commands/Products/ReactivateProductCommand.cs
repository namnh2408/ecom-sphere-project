using MediatR;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Commands.Products;

/// <summary>
/// Command to reactivate a product
/// </summary>
public record ReactivateProductCommand(
    Guid ProductId
) : IRequest<Result<Unit>>;