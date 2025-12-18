using MediatR;
using Users.Application.DTOs;

using BuildingBlocks.Abstractions;

namespace Users.Application.Commands.Auth;

/// <summary>
/// Command to login a user
/// </summary>
public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<Result<AuthTokenDto>>;
