using ShopHub.Common.Results;
using ShopHub.Services.ProductService.Application.DTOs;
using ShopHub.Services.ProductService.Domain.Entities;
using ShopHub.Services.ProductService.Domain.Repositories;

namespace ShopHub.Services.ProductService.Application.UseCases;

/// <summary>
/// Use Case: Tạo sản phẩm mới
/// </summary>
public class CreateProductUseCase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<CreateProductUseCase> _logger;

    public CreateProductUseCase(
        IProductRepository productRepository,
        ILogger<CreateProductUseCase> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// Thực hiện use case
    /// </summary>
    public async Task<Result<ProductResponseDto>> ExecuteAsync(
        CreateProductDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate SKU đã tồn tại
            if (!string.IsNullOrEmpty(dto.Sku))
            {
                var existingSku = await _productRepository.ExistsBySkuAsync(
                    dto.Sku,
                    cancellationToken);

                if (existingSku)
                {
                    _logger.LogWarning($"SKU {dto.Sku} already exists");
                    return Result<ProductResponseDto>.Failure(
                        $"Product with SKU '{dto.Sku}' already exists");
                }
            }

            // Tạo domain entity
            var product = Product.Create(
                name: dto.Name,
                price: dto.Price,
                stock: dto.Stock,
                categoryId: dto.CategoryId,
                description: dto.Description,
                sku: dto.Sku,
                imageUrl: dto.ImageUrl);

            // Save to repository
            await _productRepository.AddAsync(product, cancellationToken);

            // Map to DTO
            var response = new ProductResponseDto
            {
                ProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                Sku = product.Sku,
                CategoryId = product.CategoryId,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt
            };

            _logger.LogInformation($"Product {product.Id} created successfully");

            return Result<ProductResponseDto>.Success(
                response,
                "Product created successfully",
                201);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Validation error: {ex.Message}");
            return Result<ProductResponseDto>.Failure(ex.Message, 400);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating product: {ex.Message}");
            return Result<ProductResponseDto>.Failure(
                "An error occurred while creating the product",
                500);
        }
    }
}