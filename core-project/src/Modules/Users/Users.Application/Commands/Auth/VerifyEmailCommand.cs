using BuildingBlocks.Abstractions;
using MediatR;

namespace Users.Application.Commands.Auth;

/// <summary>
/// Command to verify user email with token
/// </summary>
public record VerifyEmailCommand(
    string Token
) : IRequest<Result<bool>>;