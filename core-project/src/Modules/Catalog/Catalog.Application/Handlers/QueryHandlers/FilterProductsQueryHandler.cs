using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.DTOs;
using Catalog.Application.Queries.Products;
using Catalog.Domain.Repositories;

namespace Catalog.Application.Handlers.QueryHandlers;

/// <summary>
/// Handler for FilterProductsQuery
/// </summary>
public class FilterProductsQueryHandler : IRequestHandler<FilterProductsQuery, Result<PaginatedProductResult>>
{
    private readonly IProductRepository _productRepository;

    public FilterProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<PaginatedProductResult>> Handle(FilterProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (products, totalCount) = await _productRepository.FilterAsync(
                request.CategoryId,
                request.MinPrice,
                request.MaxPrice,
                request.Status,
                request.InStock,
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
                totalCount,
                request.PageNumber,
                request.PageSize
            );

            return Result<PaginatedProductResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PaginatedProductResult>.Fail("FILTER_PRODUCTS_ERROR", ex.Message);
        }
    }
}