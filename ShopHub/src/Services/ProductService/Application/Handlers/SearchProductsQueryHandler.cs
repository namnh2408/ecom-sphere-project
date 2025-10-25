using ShopHub.Common.Pagination;
using ShopHub.Common.Results;
using ShopHub.Domain.CQRS;
using ShopHub.Infrastructure.Caching;
using ShopHub.Services.ProductService.Application.DTOs;
using ShopHub.Services.ProductService.Application.Queries;
using ShopHub.Services.ProductService.Domain.Repositories;

namespace ShopHub.Services.ProductService.Application.Handlers;

/// <summary>
/// Handler cho SearchProductsQuery
/// Sử dụng Dapper (IProductQueryRepository) + Redis caching
/// </summary>
public class SearchProductsQueryHandler : IQueryHandler<SearchProductsQuery, PaginatedList<ProductResponseDto>>
{
    private readonly IProductQueryRepository _queryRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SearchProductsQueryHandler> _logger;
    private const string CacheKeyPrefix = "search_products";
    private const int CacheDurationSeconds = 300; // 5 minutes

    public SearchProductsQueryHandler(
        IProductQueryRepository queryRepository,
        ICacheService cacheService,
        ILogger<SearchProductsQueryHandler> logger)
    {
        _queryRepository = queryRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Handle SearchProductsQuery với Redis caching
    /// </summary>
    public async Task<Result<PaginatedList<ProductResponseDto>>> Handle(
        SearchProductsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate search term
            if (string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                return Result<PaginatedList<ProductResponseDto>>.Failure("Search term cannot be empty");
            }

            _logger.LogInformation($"Searching products with term: {request.SearchTerm}, page: {request.PageNumber}");

            // Tạo cache key
            var cacheKey = GenerateCacheKey(request.SearchTerm, request.PageNumber, request.PageSize);

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
            var products = await _queryRepository.SearchAsync(
                request.SearchTerm,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            // Get total count for pagination
            var totalCount = await _queryRepository.GetSearchCountAsync(
                request.SearchTerm,
                cancellationToken);

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
            _logger.LogError(ex, "Error searching products");
            return Result<PaginatedList<ProductResponseDto>>.Failure($"Error searching products: {ex.Message}");
        }
    }

    /// <summary>
    /// Tạo cache key từ search parameters
    /// </summary>
    private static string GenerateCacheKey(string? searchTerm, int pageNumber, int pageSize)
    {
        var term = string.IsNullOrWhiteSpace(searchTerm) ? "all" : searchTerm.ToLower().Trim();
        return $"{CacheKeyPrefix}:{term}:page{pageNumber}:size{pageSize}";
    }
}