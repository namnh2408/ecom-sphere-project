using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.DTOs;
using Catalog.Application.Queries.Categories;
using Catalog.Domain.Repositories;

namespace Catalog.Application.Handlers.QueryHandlers;

/// <summary>
/// Handler for GetAllCategoriesQuery
/// </summary>
public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, Result<PaginatedCategoryResult>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<PaginatedCategoryResult>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (categories, totalCount) = await _categoryRepository.GetActiveAsync(
                request.PageNumber,
                request.PageSize,
                cancellationToken
            );

            var dtos = categories.Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.IsActive,
                c.CreatedAtUtc,
                c.UpdatedAtUtc
            )).ToList();

            var result = new PaginatedCategoryResult(
                dtos,
                totalCount,
                request.PageNumber,
                request.PageSize
            );

            return Result<PaginatedCategoryResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PaginatedCategoryResult>.Fail("GET_CATEGORIES_ERROR", ex.Message);
        }
    }
}