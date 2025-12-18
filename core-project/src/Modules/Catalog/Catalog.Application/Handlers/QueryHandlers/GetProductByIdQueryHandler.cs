using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.DTOs;
using Catalog.Application.Queries.Products;
using Catalog.Domain.Repositories;

namespace Catalog.Application.Handlers.QueryHandlers;

/// <summary>
/// Handler for GetProductByIdQuery
/// </summary>
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                return Result<ProductDto>.Fail("PRODUCT_NOT_FOUND", $"Product with ID {request.ProductId} not found");
            }

            var dto = new ProductDto(
                product.Id,
                product.Name,
                product.Sku.Value,
                product.Description,
                product.Price.Amount,
                product.CostPrice?.Amount,
                product.Stock.Quantity,
                product.Stock.IsLow,
                product.CategoryId,
                product.Status,
                product.PrimaryImagePath,
                product.CreatedAtUtc,
                product.UpdatedAtUtc
            );

            return Result<ProductDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ProductDto>.Fail("GET_PRODUCT_ERROR", ex.Message);
        }
    }
}