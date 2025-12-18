using FluentValidation;
using Users.Application.Commands.Auth;

namespace Users.Application.Validators;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Verification token is required")
            .MinimumLength(32).WithMessage("Verification token is invalid");
    }
}