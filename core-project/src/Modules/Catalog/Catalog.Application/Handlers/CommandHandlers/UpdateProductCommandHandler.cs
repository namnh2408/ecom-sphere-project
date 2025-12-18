using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.Commands.Products;
using Catalog.Domain.Repositories;
using FluentValidation;

namespace Catalog.Application.Handlers.CommandHandlers;

/// <summary>
/// Handler for UpdateProductCommand
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<Unit>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IValidator<UpdateProductCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IValidator<UpdateProductCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result<Unit>.Fail("VALIDATION_ERROR", errors);
        }

        try
        {
            // Get product
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                return Result<Unit>.Fail("PRODUCT_NOT_FOUND", $"Product with ID {request.ProductId} not found");
            }

            // Check if category exists
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result<Unit>.Fail("CATEGORY_NOT_FOUND", $"Category with ID {request.CategoryId} not found");
            }

            // Update product
            product.Update(
                request.Name,
                request.Description,
                request.Price,
                request.CostPrice,
                request.CategoryId,
                request.PrimaryImagePath
            );

            // Update in repository
            await _productRepository.UpdateAsync(product, cancellationToken);

            // Commit changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Fail("UPDATE_PRODUCT_ERROR", ex.Message);
        }
    }
}