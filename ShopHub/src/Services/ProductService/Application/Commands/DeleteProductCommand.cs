using FluentValidation;
using ShopHub.Common.CQRS;
using ShopHub.Domain.CQRS;

namespace ShopHub.Services.ProductService.Application.Commands;

/// <summary>
/// Command: Xóa sản phẩm (soft delete)
/// </summary>
[Transactional]
public class DeleteProductCommand : ICommand
{
    public Guid Id { get; set; }

    public DeleteProductCommand() { }

    public DeleteProductCommand(Guid id)
    {
        Id = id;
    }
}

/// <summary>
/// Validator cho DeleteProductCommand
/// </summary>
public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");
    }
}