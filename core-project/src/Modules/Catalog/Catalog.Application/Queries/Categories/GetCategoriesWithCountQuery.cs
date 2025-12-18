using MediatR;
using Catalog.Application.DTOs;
using BuildingBlocks.Abstractions;

namespace Catalog.Application.Queries.Categories;

/// <summary>
/// Query to get all categories with product count
/// </summary>
public record GetCategoriesWithCountQuery() : IRequest<Result<IEnumerable<CategoryWithCountDto>>>;