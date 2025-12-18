using FluentValidation;
using Users.Application.Commands.Auth;

namespace Users.Application.Validators;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required")
            .MinimumLength(32).WithMessage("Refresh token is invalid");
    }
}