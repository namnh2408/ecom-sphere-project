using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.Commands.Products;
using Catalog.Domain.Repositories;

namespace Catalog.Application.Handlers.CommandHandlers;

/// <summary>
/// Handler for DeactivateProductCommand
/// </summary>
public class DeactivateProductCommandHandler : IRequestHandler<DeactivateProductCommand, Result<Unit>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get product
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                return Result<Unit>.Fail("PRODUCT_NOT_FOUND", $"Product with ID {request.ProductId} not found");
            }

            // Deactivate product
            product.Deactivate();

            // Update in repository
            await _productRepository.UpdateAsync(product, cancellationToken);

            // Commit changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Fail("DEACTIVATE_PRODUCT_ERROR", ex.Message);
        }
    }
}