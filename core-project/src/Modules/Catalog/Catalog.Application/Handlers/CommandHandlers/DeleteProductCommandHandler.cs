using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.Commands.Products;
using Catalog.Domain.Repositories;

namespace Catalog.Application.Handlers.CommandHandlers;

/// <summary>
/// Handler for DeleteProductCommand
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<Unit>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get product
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                return Result<Unit>.Fail("PRODUCT_NOT_FOUND", $"Product with ID {request.ProductId} not found");
            }

            // Soft delete product
            product.SoftDelete();

            // Update in repository
            await _productRepository.UpdateAsync(product, cancellationToken);

            // Commit changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Fail("DELETE_PRODUCT_ERROR", ex.Message);
        }
    }
}