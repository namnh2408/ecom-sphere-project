using ShopHub.Common.Results;
using ShopHub.Domain.CQRS;
using ShopHub.Domain.Abstractions;
using ShopHub.Services.ProductService.Application.Commands;
using ShopHub.Services.ProductService.Domain.Repositories;

namespace ShopHub.Services.ProductService.Application.Handlers;

/// <summary>
/// Handler cho DeleteProductCommand
/// </summary>
public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handle DeleteProductCommand
    /// </summary>
    public async Task<Result> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"Deleting product: {request.Id}");

            // Get existing product
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning($"Product not found: {request.Id}");
                return Result.Failure("Product not found", 404);
            }

            // Soft delete (using IsDeleted flag)
            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;

            // Update repository
            await _productRepository.UpdateAsync(product, cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Product deleted successfully: {request.Id}");

            return Result.Success("Product deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product");
            return Result.Failure($"Error deleting product: {ex.Message}");
        }
    }
}