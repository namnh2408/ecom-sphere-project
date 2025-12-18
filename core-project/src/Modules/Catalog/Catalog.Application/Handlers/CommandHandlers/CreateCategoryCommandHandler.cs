using MediatR;
using BuildingBlocks.Abstractions;
using Catalog.Application.Commands.Categories;
using Catalog.Domain.Categories;
using Catalog.Domain.Repositories;
using FluentValidation;

namespace Catalog.Application.Handlers.CommandHandlers;

/// <summary>
/// Handler for CreateCategoryCommand
/// </summary>
public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IValidator<CreateCategoryCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IValidator<CreateCategoryCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
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
            // Check if category name already exists
            var exists = await _categoryRepository.ExistsByNameAsync(request.Name, null, cancellationToken);
            if (exists)
            {
                return Result<Guid>.Fail("DUPLICATE_CATEGORY_NAME", $"A category with name '{request.Name}' already exists");
            }

            // Create category
            var category = Category.Create(request.Name, request.Description);

            // Add to repository
            await _categoryRepository.AddAsync(category, cancellationToken);

            // Commit changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(category.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail("CREATE_CATEGORY_ERROR", ex.Message);
        }
    }
}