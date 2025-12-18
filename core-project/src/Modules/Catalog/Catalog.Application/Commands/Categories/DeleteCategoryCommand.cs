using MediatR;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Commands.Categories;

/// <summary>
/// Command to delete a category (soft delete)
/// </summary>
public record DeleteCategoryCommand(
    Guid CategoryId
) : IRequest<Result<Unit>>;