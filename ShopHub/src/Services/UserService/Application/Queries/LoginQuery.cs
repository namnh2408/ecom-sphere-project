using FluentValidation;
using ShopHub.Common.Results;
using ShopHub.Domain.CQRS;
using ShopHub.Services.UserService.Application.DTOs;

namespace ShopHub.Services.UserService.Application.Queries;

/// <summary>
/// Query: Login người dùng
/// </summary>
public class LoginQuery : IQuery<LoginResponseDto>
{
    /// <summary>
    /// Email người dùng
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu
    /// </summary>
    public string Password { get; set; } = string.Empty;

    public LoginQuery() { }

    public LoginQuery(string email, string password)
    {
        Email = email;
        Password = password;
    }
}

/// <summary>
/// Validator cho LoginQuery
/// </summary>
public class LoginQueryValidator : AbstractValidator<LoginQuery>
{
    public LoginQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}