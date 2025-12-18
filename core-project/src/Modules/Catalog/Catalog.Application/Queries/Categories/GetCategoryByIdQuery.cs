using MediatR;
using Catalog.Application.DTOs;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Queries.Categories;

/// <summary>
/// Query to get a category by ID
/// </summary>
public record GetCategoryByIdQuery(
    Guid CategoryId
) : IRequest<Result<CategoryDto>>;