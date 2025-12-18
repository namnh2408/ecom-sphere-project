using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.Commands.Products;
using Catalog.Domain.Repositories;

namespace Catalog.Application.Handlers.CommandHandlers;

/// <summary>
/// Handler for ReactivateProductCommand
/// </summary>
public class ReactivateProductCommandHandler : IRequestHandler<ReactivateProductCommand, Result<Unit>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(ReactivateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get product
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                return Result<Unit>.Fail("PRODUCT_NOT_FOUND", $"Product with ID {request.ProductId} not found");
            }

            // Reactivate product
            product.Reactivate();

            // Update in repository
            await _productRepository.UpdateAsync(product, cancellationToken);

            // Commit changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Fail("REACTIVATE_PRODUCT_ERROR", ex.Message);
        }
    }
}