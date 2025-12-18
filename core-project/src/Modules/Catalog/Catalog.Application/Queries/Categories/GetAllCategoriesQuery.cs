using MediatR;
using Catalog.Application.DTOs;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Queries.Categories;

/// <summary>
/// Query to get all categories
/// </summary>
public record GetAllCategoriesQuery(
    int PageNumber,
    int PageSize
) : IRequest<Result<PaginatedCategoryResult>>;