using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.Commands.Categories;
using Catalog.Domain.Repositories;

namespace Catalog.Application.Handlers.CommandHandlers;

/// <summary>
/// Handler for DeleteCategoryCommand
/// </summary>
public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<Unit>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get category
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result<Unit>.Fail("CATEGORY_NOT_FOUND", $"Category with ID {request.CategoryId} not found");
            }

            // Soft delete category
            category.SoftDelete();

            // Update in repository
            await _categoryRepository.UpdateAsync(category, cancellationToken);

            // Commit changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Fail("DELETE_CATEGORY_ERROR", ex.Message);
        }
    }
}