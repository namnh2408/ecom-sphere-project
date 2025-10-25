using ShopHub.Common.Results;
using ShopHub.Domain.CQRS;
using ShopHub.Infrastructure.Caching;
using ShopHub.Services.ProductService.Application.DTOs;
using ShopHub.Services.ProductService.Application.Queries;
using ShopHub.Services.ProductService.Domain.Repositories;

namespace ShopHub.Services.ProductService.Application.Handlers;

/// <summary>
/// Handler cho GetRelatedProductsQuery
/// Lấy các sản phẩm liên quan (cùng danh mục)
/// </summary>
public class GetRelatedProductsQueryHandler : IQueryHandler<GetRelatedProductsQuery, List<ProductResponseDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductQueryRepository _queryRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetRelatedProductsQueryHandler> _logger;
    private const string CacheKeyPrefix = "related_products";
    private const int CacheDurationSeconds = 600; // 10 minutes

    public GetRelatedProductsQueryHandler(
        IProductRepository productRepository,
        IProductQueryRepository queryRepository,
        ICacheService cacheService,
        ILogger<GetRelatedProductsQueryHandler> logger)
    {
        _productRepository = productRepository;
        _queryRepository = queryRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Handle GetRelatedProductsQuery
    /// </summary>
    public async Task<Result<List<ProductResponseDto>>> Handle(
        GetRelatedProductsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"Getting related products for: {request.ProductId}");

            var cacheKey = $"{CacheKeyPrefix}:{request.ProductId}:limit{request.Limit}";

            // Check cache
            var cachedResult = await _cacheService.GetAsync<List<ProductResponseDto>>(
                cacheKey,
                cancellationToken);

            if (cachedResult != null)
            {
                _logger.LogInformation($"Cache hit for related products: {request.ProductId}");
                return Result<List<ProductResponseDto>>.Success(cachedResult);
            }

            // Get current product to find its category
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null || product.IsDeleted)
            {
                _logger.LogWarning($"Product not found: {request.ProductId}");
                return Result<List<ProductResponseDto>>.Failure("Product not found", 404);
            }

            // Get related products from same category
            var relatedProducts = await _queryRepository.GetByCategoryAsync(
                product.CategoryId,
                cancellationToken);

            // Filter out current product and limit
            var filteredProducts = relatedProducts
                .Where(p => p.Id != request.ProductId && !p.IsDeleted && p.IsActive)
                .Take(request.Limit)
                .ToList();

            // Map to DTOs
            var dtos = filteredProducts.Select(p => new ProductResponseDto
            {
                ProductId = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CategoryId = p.CategoryId,
                Sku = p.Sku,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                TotalSold = p.TotalSold,
                CreatedAt = p.CreatedAt
            }).ToList();

            // Cache the result
            await _cacheService.SetAsync(
                cacheKey,
                dtos,
                TimeSpan.FromSeconds(CacheDurationSeconds),
                cancellationToken);

            _logger.LogInformation($"Found {dtos.Count} related products for: {request.ProductId}");

            return Result<List<ProductResponseDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting related products");
            return Result<List<ProductResponseDto>>.Failure($"Error getting related products: {ex.Message}");
        }
    }
}