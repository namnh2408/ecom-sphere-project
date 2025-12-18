using MediatR;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Commands.Categories;

/// <summary>
/// Command to deactivate a category
/// </summary>
public record DeactivateCategoryCommand(
    Guid CategoryId
) : IRequest<Result<Unit>>;