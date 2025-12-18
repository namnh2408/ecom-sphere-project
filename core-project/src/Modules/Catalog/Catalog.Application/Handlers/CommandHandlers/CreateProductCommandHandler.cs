using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.Commands.Products;
using Catalog.Domain.Products;
using Catalog.Domain.Repositories;
using FluentValidation;

namespace Catalog.Application.Handlers.CommandHandlers;

/// <summary>
/// Handler for CreateProductCommand
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IValidator<CreateProductCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IValidator<CreateProductCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result<Guid>.Fail("VALIDATION_ERROR", errors);
        }

        try
        {
            // Check if category exists
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result<Guid>.Fail("CATEGORY_NOT_FOUND", $"Category with ID {request.CategoryId} not found");
            }

            // Check if SKU already exists
            var existingSku = await _productRepository.ExistsBySkuAsync(request.Sku, null, cancellationToken);
            if (existingSku)
            {
                return Result<Guid>.Fail("DUPLICATE_SKU", $"A product with SKU '{request.Sku}' already exists");
            }

            // Create product
            var product = Product.Create(
                request.Name,
                request.Sku,
                request.Description,
                request.Price,
                request.CostPrice,
                request.StockQuantity,
                request.CategoryId,
                request.PrimaryImagePath
            );

            // Add to repository
            await _productRepository.AddAsync(product, cancellationToken);

            // Commit changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(product.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail("CREATE_PRODUCT_ERROR", ex.Message);
        }
    }
}