using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.DTOs;
using Catalog.Application.Queries.Categories;
using Catalog.Domain.Repositories;

namespace Catalog.Application.Handlers.QueryHandlers;

/// <summary>
/// Handler for GetCategoryByIdQuery
/// </summary>
public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result<CategoryDto>.Fail("CATEGORY_NOT_FOUND", $"Category with ID {request.CategoryId} not found");
            }

            var dto = new CategoryDto(
                category.Id,
                category.Name,
                category.Slug,
                category.Description,
                category.IsActive,
                category.CreatedAtUtc,
                category.UpdatedAtUtc
            );

            return Result<CategoryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<CategoryDto>.Fail("GET_CATEGORY_ERROR", ex.Message);
        }
    }
}