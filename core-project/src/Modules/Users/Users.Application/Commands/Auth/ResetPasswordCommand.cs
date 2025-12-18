using MediatR;

using BuildingBlocks.Abstractions;

namespace Users.Application.Commands.Auth;

/// <summary>
/// Command to reset user password (admin/recovery)
/// </summary>
public record ResetPasswordCommand(
    Guid UserId,
    string NewPassword
) : IRequest<Result<Unit>>;
