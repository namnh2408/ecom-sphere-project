using MediatR;
using Users.Application.DTOs;

using BuildingBlocks.Abstractions;

namespace Users.Application.Commands.Users;

/// <summary>
/// Command to update user profile
/// </summary>
public record UpdateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName
) : IRequest<Result<UserDto>>;
