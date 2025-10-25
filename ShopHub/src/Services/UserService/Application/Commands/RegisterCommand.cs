using FluentValidation;
using ShopHub.Common.CQRS;
using ShopHub.Domain.CQRS;
using ShopHub.Services.UserService.Application.DTOs;

namespace ShopHub.Services.UserService.Application.Commands;

/// <summary>
/// Command: Đăng ký người dùng mới
/// </summary>
[Transactional]
public class RegisterCommand : ICommand<UserProfileDto>
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    public RegisterCommand() { }

    public RegisterCommand(
        string email,
        string fullName,
        string password,
        string confirmPassword,
        string? phoneNumber = null)
    {
        Email = email;
        FullName = fullName;
        Password = password;
        ConfirmPassword = confirmPassword;
        PhoneNumber = phoneNumber;
    }
}

/// <summary>
/// Validator cho RegisterCommand
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MinimumLength(3).WithMessage("Full name must be at least 3 characters")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9\-\s]{10,}$").WithMessage("Invalid phone number")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}