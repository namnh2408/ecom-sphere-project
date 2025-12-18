using MediatR;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Commands.Categories;

/// <summary>
/// Command to update a category
/// </summary>
public record UpdateCategoryCommand(
    Guid CategoryId,
    string Name,
    string? Description
) : IRequest<Result<Unit>>;