using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.Commands.Products;
using Catalog.Domain.Repositories;
using FluentValidation;

namespace Catalog.Application.Handlers.CommandHandlers;

/// <summary>
/// Handler for UpdateProductStockCommand
/// </summary>
public class UpdateProductStockCommandHandler : IRequestHandler<UpdateProductStockCommand, Result<Unit>>
{
    private readonly IProductRepository _productRepository;
    private readonly IValidator<UpdateProductStockCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductStockCommandHandler(
        IProductRepository productRepository,
        IValidator<UpdateProductStockCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
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

            // Update stock
            product.UpdateStock(request.NewQuantity, request.Reason);

            // Update in repository
            await _productRepository.UpdateAsync(product, cancellationToken);

            // Commit changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Fail("UPDATE_STOCK_ERROR", ex.Message);
        }
    }
}