using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.Commands.Categories;
using Catalog.Domain.Repositories;
using FluentValidation;

namespace Catalog.Application.Handlers.CommandHandlers;

/// <summary>
/// Handler for UpdateCategoryCommand
/// </summary>
public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<Unit>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IValidator<UpdateCategoryCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IValidator<UpdateCategoryCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
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
            // Get category
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result<Unit>.Fail("CATEGORY_NOT_FOUND", $"Category with ID {request.CategoryId} not found");
            }

            // Check if new name already exists (exclude current category)
            var exists = await _categoryRepository.ExistsByNameAsync(request.Name, request.CategoryId, cancellationToken);
            if (exists)
            {
                return Result<Unit>.Fail("DUPLICATE_CATEGORY_NAME", $"A category with name '{request.Name}' already exists");
            }

            // Update category
            category.Update(request.Name, request.Description);

            // Update in repository
            await _categoryRepository.UpdateAsync(category, cancellationToken);

            // Commit changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Fail("UPDATE_CATEGORY_ERROR", ex.Message);
        }
    }
}