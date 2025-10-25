using FluentValidation;
using ShopHub.Common.CQRS;
using ShopHub.Domain.CQRS;
using ShopHub.Services.ProductService.Application.DTOs;

namespace ShopHub.Services.ProductService.Application.Commands;

/// <summary>
/// Command: Cập nhật sản phẩm
/// </summary>
[Transactional]
public class UpdateProductCommand : ICommand<ProductResponseDto>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? ImageUrl { get; set; }

    public UpdateProductCommand() { }

    public UpdateProductCommand(
        Guid id,
        string? name = null,
        string? description = null,
        decimal? price = null,
        string? imageUrl = null)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        ImageUrl = imageUrl;
    }
}

/// <summary>
/// Validator cho UpdateProductCommand
/// </summary>
public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Name)
            .MinimumLength(3).WithMessage("Product name must be at least 3 characters")
            .MaximumLength(255).WithMessage("Product name must not exceed 255 characters")
            .When(x => x.Name != null);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => x.Description != null);
    }
}