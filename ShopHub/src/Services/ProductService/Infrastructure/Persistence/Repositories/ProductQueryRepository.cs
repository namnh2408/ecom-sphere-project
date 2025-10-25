using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ShopHub.Services.ProductService.Domain.Entities;
using ShopHub.Services.ProductService.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ShopHub.Services.ProductService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Query Repository implementation sử dụng Dapper
/// Tối ưu hóa cho read operations
/// </summary>
public class ProductQueryRepository : IProductQueryRepository
{
    private readonly string _connectionString;
    private readonly ILogger<ProductQueryRepository> _logger;

    public ProductQueryRepository(
        IConfiguration configuration,
        ILogger<ProductQueryRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("ProductDb")
            ?? throw new InvalidOperationException("ProductDb connection string not found");
        _logger = logger;
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    /// <summary>
    /// Tìm kiếm sản phẩm với phân trang
    /// </summary>
    public async Task<IReadOnlyList<Product>> SearchAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (var connection = CreateConnection())
            {
                var offset = (pageNumber - 1) * pageSize;

                var sql = @"
                    SELECT 
                        p.Id, p.Name, p.Description, p.Price, p.Stock, 
                        p.CategoryId, p.Sku, p.ImageUrl, p.IsActive, 
                        p.TotalSold, p.CreatedAt, p.UpdatedAt, p.IsDeleted
                    FROM Products p
                    WHERE p.IsDeleted = 0 AND p.IsActive = 1";

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    sql += @" AND (
                        p.Name LIKE @SearchTerm 
                        OR p.Description LIKE @SearchTerm 
                        OR p.Sku LIKE @SearchTerm)";
                }

                sql += @"
                    ORDER BY p.Name ASC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                var parameters = new
                {
                    SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm}%",
                    Offset = offset,
                    PageSize = pageSize
                };

                var products = await connection.QueryAsync<Product>(sql, parameters);
                _logger.LogDebug($"SearchAsync: Found {products.Count()} products for term: {searchTerm}");

                return products.ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in SearchAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Lấy tổng số sản phẩm tìm kiếm
    /// </summary>
    public async Task<int> GetSearchCountAsync(
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT COUNT(*) 
                    FROM Products p
                    WHERE p.IsDeleted = 0 AND p.IsActive = 1";

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    sql += @" AND (
                        p.Name LIKE @SearchTerm 
                        OR p.Description LIKE @SearchTerm 
                        OR p.Sku LIKE @SearchTerm)";
                }

                var parameters = new
                {
                    SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm}%"
                };

                var count = await connection.ExecuteScalarAsync<int>(sql, parameters);
                _logger.LogDebug($"GetSearchCountAsync: Total {count} products for term: {searchTerm}");

                return count;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in GetSearchCountAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Lấy sản phẩm theo ID
    /// </summary>
    public async Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT 
                        Id, Name, Description, Price, Stock, 
                        CategoryId, Sku, ImageUrl, IsActive, 
                        TotalSold, CreatedAt, UpdatedAt, IsDeleted
                    FROM Products
                    WHERE Id = @Id AND IsDeleted = 0";

                var product = await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
                _logger.LogDebug($"GetByIdAsync: Found product {id}");

                return product;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in GetByIdAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Lấy sản phẩm theo SKU
    /// </summary>
    public async Task<Product?> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT 
                        Id, Name, Description, Price, Stock, 
                        CategoryId, Sku, ImageUrl, IsActive, 
                        TotalSold, CreatedAt, UpdatedAt, IsDeleted
                    FROM Products
                    WHERE Sku = @Sku AND IsDeleted = 0";

                var product = await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Sku = sku });
                _logger.LogDebug($"GetBySkuAsync: Found product with SKU {sku}");

                return product;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in GetBySkuAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Lấy sản phẩm theo danh mục
    /// </summary>
    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT 
                        Id, Name, Description, Price, Stock, 
                        CategoryId, Sku, ImageUrl, IsActive, 
                        TotalSold, CreatedAt, UpdatedAt, IsDeleted
                    FROM Products
                    WHERE CategoryId = @CategoryId AND IsDeleted = 0 AND IsActive = 1
                    ORDER BY Name ASC";

                var products = await connection.QueryAsync<Product>(sql, new { CategoryId = categoryId });
                _logger.LogDebug($"GetByCategoryAsync: Found {products.Count()} products for category {categoryId}");

                return products.ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in GetByCategoryAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Lấy sản phẩm nổi bật (best sellers)
    /// </summary>
    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT TOP(@Limit)
                        Id, Name, Description, Price, Stock, 
                        CategoryId, Sku, ImageUrl, IsActive, 
                        TotalSold, CreatedAt, UpdatedAt, IsDeleted
                    FROM Products
                    WHERE IsDeleted = 0 AND IsActive = 1
                    ORDER BY TotalSold DESC";

                var products = await connection.QueryAsync<Product>(sql, new { Limit = limit });
                _logger.LogDebug($"GetFeaturedAsync: Found {products.Count()} featured products");

                return products.ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in GetFeaturedAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Kiểm tra sản phẩm có tồn tại
    /// </summary>
    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (var connection = CreateConnection())
            {
                var sql = "SELECT CAST(COUNT(*) AS BIT) FROM Products WHERE Id = @Id AND IsDeleted = 0";
                var exists = await connection.ExecuteScalarAsync<bool>(sql, new { Id = id });
                _logger.LogDebug($"ExistsAsync: Product {id} exists = {exists}");

                return exists;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in ExistsAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Đếm sản phẩm theo danh mục
    /// </summary>
    public async Task<int> CountByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (var connection = CreateConnection())
            {
                var sql = "SELECT COUNT(*) FROM Products WHERE CategoryId = @CategoryId AND IsDeleted = 0";
                var count = await connection.ExecuteScalarAsync<int>(sql, new { CategoryId = categoryId });
                _logger.LogDebug($"CountByCategoryAsync: Found {count} products for category {categoryId}");

                return count;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in CountByCategoryAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Lấy tất cả sản phẩm với phân trang
    /// </summary>
    public async Task<IReadOnlyList<Product>> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        string? sortDirection = "asc",
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (var connection = CreateConnection())
            {
                var offset = (pageNumber - 1) * pageSize;
                var orderBy = !string.IsNullOrEmpty(sortBy) ? $"ORDER BY p.{sortBy} {sortDirection}" : "ORDER BY p.CreatedAt DESC";

                var whereClause = "WHERE p.IsDeleted = 0";
                if (isActive.HasValue)
                {
                    whereClause += $" AND p.IsActive = {(isActive.Value ? 1 : 0)}";
                }

                var sql = $@"
                    SELECT 
                        p.Id, p.Name, p.Description, p.Price, p.Stock, 
                        p.CategoryId, p.Sku, p.ImageUrl, p.IsActive, 
                        p.TotalSold, p.CreatedAt, p.UpdatedAt, p.IsDeleted
                    FROM Products p
                    {whereClause}
                    {orderBy}
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                var products = await connection.QueryAsync<Product>(sql, new { Offset = offset, PageSize = pageSize });
                _logger.LogDebug($"GetAllAsync: Retrieved {products.Count()} products for page {pageNumber}");

                return products.ToList().AsReadOnly();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in GetAllAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Đếm tổng số sản phẩm
    /// </summary>
    public async Task<int> GetTotalCountAsync(
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (var connection = CreateConnection())
            {
                var whereClause = "WHERE IsDeleted = 0";
                if (isActive.HasValue)
                {
                    whereClause += $" AND IsActive = {(isActive.Value ? 1 : 0)}";
                }

                var sql = $"SELECT COUNT(*) FROM Products {whereClause}";
                var count = await connection.ExecuteScalarAsync<int>(sql);
                _logger.LogDebug($"GetTotalCountAsync: Total count = {count}");

                return count;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in GetTotalCountAsync: {ex.Message}");
            throw;
        }
    }
}