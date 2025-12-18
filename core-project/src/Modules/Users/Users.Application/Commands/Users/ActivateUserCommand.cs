using MediatR;

using BuildingBlocks.Abstractions;

namespace Users.Application.Commands.Users;

/// <summary>
/// Command to activate a user account
/// </summary>
public record ActivateUserCommand(
    Guid UserId
) : IRequest<Result<Unit>>;
