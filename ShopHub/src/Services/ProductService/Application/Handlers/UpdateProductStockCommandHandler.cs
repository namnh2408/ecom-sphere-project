using ShopHub.Common.Results;
using ShopHub.Domain.CQRS;
using ShopHub.Domain.Abstractions;
using ShopHub.Services.ProductService.Application.Commands;
using ShopHub.Services.ProductService.Application.DTOs;
using ShopHub.Services.ProductService.Domain.Repositories;

namespace ShopHub.Services.ProductService.Application.Handlers;

/// <summary>
/// Handler cho UpdateProductStockCommand
/// </summary>
public class UpdateProductStockCommandHandler : ICommandHandler<UpdateProductStockCommand, ProductResponseDto>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProductStockCommandHandler> _logger;

    public UpdateProductStockCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateProductStockCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handle UpdateProductStockCommand
    /// </summary>
    public async Task<Result<ProductResponseDto>> Handle(
        UpdateProductStockCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"Updating stock for product: {request.ProductId}, operation: {request.Operation}, quantity: {request.Quantity}");

            // Get existing product
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null || product.IsDeleted)
            {
                _logger.LogWarning($"Product not found: {request.ProductId}");
                return Result<ProductResponseDto>.Failure("Product not found", 404);
            }

            // Update stock based on operation
            switch (request.Operation)
            {
                case StockOperation.Add:
                    product.AddStock(request.Quantity);
                    _logger.LogInformation($"Added {request.Quantity} to stock. New stock: {product.Stock}");
                    break;

                case StockOperation.Subtract:
                    if (product.Stock < request.Quantity)
                    {
                        _logger.LogWarning($"Insufficient stock. Current: {product.Stock}, Required: {request.Quantity}");
                        return Result<ProductResponseDto>.Failure("Insufficient stock", 400);
                    }
                    product.ReduceStock(request.Quantity);
                    _logger.LogInformation($"Reduced stock by {request.Quantity}. New stock: {product.Stock}");
                    break;

                case StockOperation.Set:
                    product.SetStock(request.Quantity);
                    _logger.LogInformation($"Set stock to {request.Quantity}");
                    break;

                default:
                    return Result<ProductResponseDto>.Failure("Invalid stock operation", 400);
            }

            // Update repository
            await _productRepository.UpdateAsync(product, cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Product stock updated successfully: {product.Id}");

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

            return Result<ProductResponseDto>.Success(response, "Stock updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product stock");
            return Result<ProductResponseDto>.Failure($"Error updating stock: {ex.Message}");
        }
    }
}