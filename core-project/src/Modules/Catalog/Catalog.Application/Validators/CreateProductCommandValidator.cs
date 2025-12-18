using FluentValidation;
using Catalog.Application.Commands.Products;

namespace Catalog.Application.Validators;

/// <summary>
/// Validator for CreateProductCommand
/// </summary>
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MinimumLength(3).WithMessage("Product name must be at least 3 characters")
            .MaximumLength(500).WithMessage("Product name must not exceed 500 characters");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required")
            .MinimumLength(3).WithMessage("SKU must be at least 3 characters")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price must be greater than or equal to 0")
            .When(x => x.CostPrice.HasValue);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be greater than or equal to 0");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}