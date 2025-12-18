using FluentValidation;
using Users.Application.Commands.Auth;

namespace Users.Application.Validators;

public class ResetPasswordWithTokenCommandValidator : AbstractValidator<ResetPasswordWithTokenCommand>
{
    public ResetPasswordWithTokenCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required")
            .MinimumLength(32).WithMessage("Reset token is invalid");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit");
    }
}