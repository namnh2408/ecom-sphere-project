using FluentValidation;
using Catalog.Application.Commands.Products;

namespace Catalog.Application.Validators;

/// <summary>
/// Validator for UpdateProductStockCommand
/// </summary>
public class UpdateProductStockCommandValidator : AbstractValidator<UpdateProductStockCommand>
{
    public UpdateProductStockCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.NewQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be greater than or equal to 0");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(200).WithMessage("Reason must not exceed 200 characters");
    }
}