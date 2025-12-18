using MediatR;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Commands.Categories;

/// <summary>
/// Command to create a new category
/// </summary>
public record CreateCategoryCommand(
    string Name,
    string? Description
) : IRequest<Result<Guid>>;