using FluentValidation;
using ShopHub.Common.CQRS;
using ShopHub.Domain.CQRS;
using ShopHub.Services.ProductService.Application.DTOs;

namespace ShopHub.Services.ProductService.Application.Commands;

/// <summary>
/// Command: Cập nhật số tồn kho sản phẩm
/// </summary>
[Transactional]
public class UpdateProductStockCommand : ICommand<ProductResponseDto>
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public StockOperation Operation { get; set; }

    public UpdateProductStockCommand() { }

    public UpdateProductStockCommand(Guid productId, int quantity, StockOperation operation)
    {
        ProductId = productId;
        Quantity = quantity;
        Operation = operation;
    }
}

/// <summary>
/// Loại hoạt động với stock
/// </summary>
public enum StockOperation
{
    /// <summary>
    /// Thêm vào tồn kho
    /// </summary>
    Add = 0,

    /// <summary>
    /// Giảm tồn kho
    /// </summary>
    Subtract = 1,

    /// <summary>
    /// Đặt số lượng tuyệt đối
    /// </summary>
    Set = 2
}

/// <summary>
/// Validator cho UpdateProductStockCommand
/// </summary>
public class UpdateProductStockCommandValidator : AbstractValidator<UpdateProductStockCommand>
{
    public UpdateProductStockCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.Operation)
            .IsInEnum().WithMessage("Operation must be a valid stock operation");
    }
}