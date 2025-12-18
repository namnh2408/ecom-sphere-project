using BuildingBlocks.Abstractions;
using MediatR;

namespace Users.Application.Commands.Auth;

/// <summary>
/// Command to request password reset
/// </summary>
public record RequestPasswordResetCommand(
    string Email
) : IRequest<Result<string>>;