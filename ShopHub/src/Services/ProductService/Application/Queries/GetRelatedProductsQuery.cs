using FluentValidation;
using ShopHub.Common.CQRS;
using ShopHub.Domain.CQRS;
using ShopHub.Services.ProductService.Application.DTOs;

namespace ShopHub.Services.ProductService.Application.Queries;

/// <summary>
/// Query: Lấy sản phẩm liên quan (cùng danh mục, không bao gồm sản phẩm hiện tại)
/// </summary>
[Cacheable(durationSeconds: 600)] // Cache 10 minutes
public class GetRelatedProductsQuery : IQuery<List<ProductResponseDto>>
{
    public Guid ProductId { get; set; }
    public int Limit { get; set; } = 5;

    public GetRelatedProductsQuery() { }

    public GetRelatedProductsQuery(Guid productId, int limit = 5)
    {
        ProductId = productId;
        Limit = limit;
    }
}

/// <summary>
/// Validator cho GetRelatedProductsQuery
/// </summary>
public class GetRelatedProductsQueryValidator : AbstractValidator<GetRelatedProductsQuery>
{
    public GetRelatedProductsQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit must be greater than 0")
            .LessThanOrEqualTo(50).WithMessage("Limit must not exceed 50");
    }
}