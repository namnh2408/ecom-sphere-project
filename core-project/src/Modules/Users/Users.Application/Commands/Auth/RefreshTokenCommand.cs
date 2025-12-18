using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.DTOs;

namespace Users.Application.Commands.Auth;

/// <summary>
/// Command to refresh JWT token
/// </summary>
public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<Result<RefreshTokenDto>>;