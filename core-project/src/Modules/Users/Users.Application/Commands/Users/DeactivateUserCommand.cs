using MediatR;

using BuildingBlocks.Abstractions;

namespace Users.Application.Commands.Users;

/// <summary>
/// Command to deactivate a user account
/// </summary>
public record DeactivateUserCommand(
    Guid UserId
) : IRequest<Result<Unit>>;
