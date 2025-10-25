using FluentValidation;
using ShopHub.Common.CQRS;
using ShopHub.Domain.CQRS;
using ShopHub.Services.ProductService.Application.DTOs;

namespace ShopHub.Services.ProductService.Application.Queries;

/// <summary>
/// Query: Lấy thông tin sản phẩm theo ID
/// </summary>
[Cacheable(durationSeconds: 600)] // Cache 10 minutes
public class GetProductByIdQuery : IQuery<ProductResponseDto>
{
    public Guid Id { get; set; }

    public GetProductByIdQuery() { }

    public GetProductByIdQuery(Guid id)
    {
        Id = id;
    }
}

/// <summary>
/// Validator cho GetProductByIdQuery
/// </summary>
public class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");
    }
}