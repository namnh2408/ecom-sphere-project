using ShopHub.Common.Pagination;
using ShopHub.Common.Results;
using ShopHub.Domain.CQRS;
using ShopHub.Infrastructure.Caching;
using ShopHub.Services.ProductService.Application.DTOs;
using ShopHub.Services.ProductService.Application.Queries;
using ShopHub.Services.ProductService.Domain.Repositories;

namespace ShopHub.Services.ProductService.Application.Handlers;

/// <summary>
/// Handler cho GetAllProductsQuery
/// Sử dụng Dapper (IProductQueryRepository) + Redis caching
/// </summary>
public class GetAllProductsQueryHandler : IQueryHandler<GetAllProductsQuery, PaginatedList<ProductResponseDto>>
{
    private readonly IProductQueryRepository _queryRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetAllProductsQueryHandler> _logger;
    private const string CacheKeyPrefix = "all_products";
    private const int CacheDurationSeconds = 300; // 5 minutes

    public GetAllProductsQueryHandler(
        IProductQueryRepository queryRepository,
        ICacheService cacheService,
        ILogger<GetAllProductsQueryHandler> logger)
    {
        _queryRepository = queryRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Handle GetAllProductsQuery với Redis caching
    /// </summary>
    public async Task<Result<PaginatedList<ProductResponseDto>>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"Getting all products, page: {request.PageNumber}, pageSize: {request.PageSize}");

            // Tạo cache key
            var cacheKey = GenerateCacheKey(request.PageNumber, request.PageSize, request.IsActive);

            // Kiểm tra Redis cache
            var cachedResult = await _cacheService.GetAsync<PaginatedList<ProductResponseDto>>(
                cacheKey,
                cancellationToken);

            if (cachedResult != null)
            {
                _logger.LogInformation($"Cache hit for key: {cacheKey}");
                return Result<PaginatedList<ProductResponseDto>>.Success(cachedResult);
            }

            _logger.LogInformation($"Cache miss for key: {cacheKey}");

            // Get products from Dapper query repository
            var products = await _queryRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDirection,
                request.IsActive,
                cancellationToken);

            // Get total count for pagination
            var totalCount = await _queryRepository.GetTotalCountAsync(
                isActive: request.IsActive,
                cancellationToken: cancellationToken);

            // Map to DTOs
            var dtos = products.Select(p => new ProductResponseDto
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

            // Create paginated result
            var result = new PaginatedList<ProductResponseDto>(
                dtos,
                totalCount,
                request.PageNumber,
                request.PageSize);

            // Cache the result in Redis
            await _cacheService.SetAsync(
                cacheKey,
                result,
                TimeSpan.FromSeconds(CacheDurationSeconds),
                cancellationToken);

            _logger.LogInformation($"Found {dtos.Count} products from database");

            return Result<PaginatedList<ProductResponseDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            return Result<PaginatedList<ProductResponseDto>>.Failure($"Error getting products: {ex.Message}");
        }
    }

    /// <summary>
    /// Tạo cache key từ pagination parameters
    /// </summary>
    private static string GenerateCacheKey(int pageNumber, int pageSize, bool? isActive)
    {
        var activeFilter = isActive == null ? "all" : (isActive.Value ? "active" : "inactive");
        return $"{CacheKeyPrefix}:page{pageNumber}:size{pageSize}:{activeFilter}";
    }
}