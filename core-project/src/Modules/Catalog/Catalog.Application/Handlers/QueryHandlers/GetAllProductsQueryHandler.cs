using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.DTOs;
using Catalog.Application.Queries.Products;
using Catalog.Domain.Repositories;

namespace Catalog.Application.Handlers.QueryHandlers;

/// <summary>
/// Handler for GetAllProductsQuery
/// </summary>
public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<PaginatedProductResult>>
{
    private readonly IProductRepository _productRepository;

    public GetAllProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<PaginatedProductResult>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (products, totalCount) = await _productRepository.GetAllAsync(
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
            return Result<PaginatedProductResult>.Fail("GET_PRODUCTS_ERROR", ex.Message);
        }
    }
}