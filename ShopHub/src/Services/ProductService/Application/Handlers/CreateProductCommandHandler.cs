using ShopHub.Common.Results;
using ShopHub.Domain.CQRS;
using ShopHub.Domain.Abstractions;
using ShopHub.Services.ProductService.Application.Commands;
using ShopHub.Services.ProductService.Application.DTOs;
using ShopHub.Services.ProductService.Domain.Entities;
using ShopHub.Services.ProductService.Domain.Repositories;

namespace ShopHub.Services.ProductService.Application.Handlers;

/// <summary>
/// Handler cho CreateProductCommand
/// </summary>
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, ProductResponseDto>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handle CreateProductCommand
    /// </summary>
    public async Task<Result<ProductResponseDto>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"Creating new product: {request.Name}");

            // Create product using factory method (domain logic)
            var product = Product.Create(
                request.Name,
                request.Price,
                request.Stock,
                request.CategoryId,
                request.Description,
                request.Sku,
                request.ImageUrl);

            // Add to repository
            await _productRepository.AddAsync(product, cancellationToken);

            // Publish domain events
            var domainEvents = product.DomainEvents;
            product.ClearDomainEvents();

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Product created successfully: {product.Id}");

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

            return Result<ProductResponseDto>.Success(response, "Product created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return Result<ProductResponseDto>.Failure($"Error creating product: {ex.Message}");
        }
    }
}