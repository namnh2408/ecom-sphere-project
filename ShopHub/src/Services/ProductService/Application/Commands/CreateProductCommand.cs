using FluentValidation;
using ShopHub.Common.CQRS;
using ShopHub.Domain.CQRS;
using ShopHub.Services.ProductService.Application.DTOs;

namespace ShopHub.Services.ProductService.Application.Commands;

/// <summary>
/// Command: Tạo sản phẩm mới
/// </summary>
[Transactional]
public class CreateProductCommand : ICommand<ProductResponseDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public Guid CategoryId { get; set; }
    public string? Sku { get; set; }
    public string? ImageUrl { get; set; }

    public CreateProductCommand() { }

    public CreateProductCommand(
        string name,
        decimal price,
        int stock,
        Guid categoryId,
        string? description = null,
        string? sku = null,
        string? imageUrl = null)
    {
        Name = name;
        Price = price;
        Stock = stock;
        CategoryId = categoryId;
        Description = description;
        Sku = sku;
        ImageUrl = imageUrl;
    }
}

/// <summary>
/// Validator cho CreateProductCommand
/// </summary>
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MinimumLength(3).WithMessage("Product name must be at least 3 characters")
            .MaximumLength(255).WithMessage("Product name must not exceed 255 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock must be non-negative");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.Sku)
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters")
            .When(x => x.Sku != null);
    }
}