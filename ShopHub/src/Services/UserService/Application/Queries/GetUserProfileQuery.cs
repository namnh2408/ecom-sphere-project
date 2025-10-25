using FluentValidation;
using ShopHub.Domain.CQRS;
using ShopHub.Services.UserService.Application.DTOs;

namespace ShopHub.Services.UserService.Application.Queries;

/// <summary>
/// Query: Lấy profile của người dùng
/// </summary>
public class GetUserProfileQuery : IQuery<UserProfileDto>
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }

    public GetUserProfileQuery() { }

    public GetUserProfileQuery(Guid userId)
    {
        UserId = userId;
    }
}

/// <summary>
/// Validator cho GetUserProfileQuery
/// </summary>
public class GetUserProfileQueryValidator : AbstractValidator<GetUserProfileQuery>
{
    public GetUserProfileQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}