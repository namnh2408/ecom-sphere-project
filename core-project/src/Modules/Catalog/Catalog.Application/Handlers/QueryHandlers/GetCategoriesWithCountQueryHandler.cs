using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.DTOs;
using Catalog.Application.Queries.Categories;
using Catalog.Domain.Repositories;

namespace Catalog.Application.Handlers.QueryHandlers;

/// <summary>
/// Handler for GetCategoriesWithCountQuery
/// </summary>
public class GetCategoriesWithCountQueryHandler : IRequestHandler<GetCategoriesWithCountQuery, Result<IEnumerable<CategoryWithCountDto>>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesWithCountQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<IEnumerable<CategoryWithCountDto>>> Handle(GetCategoriesWithCountQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var categoriesWithCount = await _categoryRepository.GetWithProductCountAsync(cancellationToken);

            var dtos = categoriesWithCount.Select(x => new CategoryWithCountDto(
                x.Category.Id,
                x.Category.Name,
                x.Category.Slug,
                x.Category.Description,
                x.Category.IsActive,
                x.ProductCount,
                x.Category.CreatedAtUtc,
                x.Category.UpdatedAtUtc
            )).ToList();

            return Result<IEnumerable<CategoryWithCountDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<CategoryWithCountDto>>.Fail("GET_CATEGORIES_WITH_COUNT_ERROR", ex.Message);
        }
    }
}