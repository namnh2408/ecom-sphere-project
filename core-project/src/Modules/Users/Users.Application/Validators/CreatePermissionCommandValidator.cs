using FluentValidation;
using Users.Application.Commands.Permissions;

namespace Users.Application.Validators;

public class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Permission name is required")
            .MaximumLength(100).WithMessage("Permission name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Resource)
            .NotEmpty().WithMessage("Resource is required")
            .MaximumLength(100).WithMessage("Resource cannot exceed 100 characters");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required")
            .MaximumLength(100).WithMessage("Action cannot exceed 100 characters");
    }
}