using MediatR;

using BuildingBlocks.Abstractions;

namespace Users.Application.Commands.Auth;

/// <summary>
/// Command to change user password
/// </summary>
public record ChangePasswordCommand(
    Guid UserId,
    string OldPassword,
    string NewPassword
) : IRequest<Result<Unit>>;
