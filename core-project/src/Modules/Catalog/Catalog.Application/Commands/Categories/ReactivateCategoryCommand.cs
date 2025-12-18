using MediatR;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Commands.Categories;

/// <summary>
/// Command to reactivate a category
/// </summary>
public record ReactivateCategoryCommand(
    Guid CategoryId
) : IRequest<Result<Unit>>;