using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.DTOs;
using Catalog.Application.Queries.Products;
using Catalog.Domain.Repositories;

namespace Catalog.Application.Handlers.QueryHandlers;

/// <summary>
/// Handler for GetProductsByCategoryQuery
/// </summary>
public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, Result<PaginatedProductResult>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public GetProductsByCategoryQueryHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<PaginatedProductResult>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify category exists
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result<PaginatedProductResult>.Fail("CATEGORY_NOT_FOUND", $"Category with ID {request.CategoryId} not found");
            }

            var products = await _productRepository.GetByCategoryIdAsync(
                request.CategoryId,
                request.PageNumber,
                request.PageSize,
                cancellationToken
            );

            var dtos = products.Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Sku.Value,
                p.Description,
                p.Price.Amount,
                p.CostPrice?.Amount,
                p.Stock.Quantity,
                p.Stock.IsLow,
                p.CategoryId,
                p.Status,
                p.PrimaryImagePath,
                p.CreatedAtUtc,
                p.UpdatedAtUtc
            )).ToList();

            var result = new PaginatedProductResult(
                dtos,
                dtos.Count,
                request.PageNumber,
                request.PageSize
            );

            return Result<PaginatedProductResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PaginatedProductResult>.Fail("GET_PRODUCTS_BY_CATEGORY_ERROR", ex.Message);
        }
    }
}