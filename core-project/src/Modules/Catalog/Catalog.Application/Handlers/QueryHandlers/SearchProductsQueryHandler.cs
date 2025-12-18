using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.DTOs;
using Catalog.Application.Queries.Products;
using Catalog.Domain.Repositories;

namespace Catalog.Application.Handlers.QueryHandlers;

/// <summary>
/// Handler for SearchProductsQuery
/// </summary>
public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, Result<PaginatedProductResult>>
{
    private readonly IProductRepository _productRepository;

    public SearchProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<PaginatedProductResult>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                return Result<PaginatedProductResult>.Fail("INVALID_SEARCH_TERM", "Search term cannot be empty");
            }

            var (products, totalCount) = await _productRepository.SearchAsync(
                request.SearchTerm,
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
            return Result<PaginatedProductResult>.Fail("SEARCH_PRODUCTS_ERROR", ex.Message);
        }
    }
}