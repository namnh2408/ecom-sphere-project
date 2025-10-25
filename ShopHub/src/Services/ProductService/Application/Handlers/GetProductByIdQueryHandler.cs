using ShopHub.Common.Results;
using ShopHub.Domain.CQRS;
using ShopHub.Services.ProductService.Application.DTOs;
using ShopHub.Services.ProductService.Application.Queries;
using ShopHub.Services.ProductService.Domain.Repositories;

namespace ShopHub.Services.ProductService.Application.Handlers;

/// <summary>
/// Handler cho GetProductByIdQuery
/// </summary>
public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductResponseDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetProductByIdQueryHandler> _logger;

    public GetProductByIdQueryHandler(
        IProductRepository productRepository,
        ILogger<GetProductByIdQueryHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handle GetProductByIdQuery
    /// </summary>
    public async Task<Result<ProductResponseDto>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"Getting product: {request.Id}");

            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

            if (product == null || product.IsDeleted)
            {
                _logger.LogWarning($"Product not found: {request.Id}");
                return Result<ProductResponseDto>.Failure("Product not found", 404);
            }

            var response = new ProductResponseDto
            {
                ProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                Sku = product.Sku,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                TotalSold = product.TotalSold,
                CreatedAt = product.CreatedAt
            };

            _logger.LogInformation($"Product retrieved successfully: {product.Id}");

            return Result<ProductResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product");
            return Result<ProductResponseDto>.Failure($"Error getting product: {ex.Message}");
        }
    }
}