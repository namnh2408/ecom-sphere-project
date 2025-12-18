using FluentValidation;
using Catalog.Application.Commands.Products;

namespace Catalog.Application.Validators;

/// <summary>
/// Validator for UpdateProductCommand
/// </summary>
public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MinimumLength(3).WithMessage("Product name must be at least 3 characters")
            .MaximumLength(500).WithMessage("Product name must not exceed 500 characters");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price must be greater than or equal to 0")
            .When(x => x.CostPrice.HasValue);

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}