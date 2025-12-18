using BuildingBlocks.Abstractions;
using MediatR;

namespace Users.Application.Commands.Auth;

/// <summary>
/// Command to reset password with reset token
/// </summary>
public record ResetPasswordWithTokenCommand(
    string Token,
    string NewPassword
) : IRequest<Result<bool>>;