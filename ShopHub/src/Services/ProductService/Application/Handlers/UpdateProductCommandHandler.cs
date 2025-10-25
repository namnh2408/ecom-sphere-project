using ShopHub.Common.Results;
using ShopHub.Domain.CQRS;
using ShopHub.Domain.Abstractions;
using ShopHub.Services.ProductService.Application.Commands;
using ShopHub.Services.ProductService.Application.DTOs;
using ShopHub.Services.ProductService.Domain.Repositories;

namespace ShopHub.Services.ProductService.Application.Handlers;

/// <summary>
/// Handler cho UpdateProductCommand
/// </summary>
public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, ProductResponseDto>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handle UpdateProductCommand
    /// </summary>
    public async Task<Result<ProductResponseDto>> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"Updating product: {request.Id}");

            // Get existing product
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning($"Product not found: {request.Id}");
                return Result<ProductResponseDto>.Failure("Product not found", 404);
            }

            // Update product using domain logic
            product.Update(
                request.Name,
                request.Description,
                request.Price,
                request.ImageUrl);

            // Update repository
            await _productRepository.UpdateAsync(product, cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Product updated successfully: {product.Id}");

            // Map to DTO
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

            return Result<ProductResponseDto>.Success(response, "Product updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product");
            return Result<ProductResponseDto>.Failure($"Error updating product: {ex.Message}");
        }
    }
}