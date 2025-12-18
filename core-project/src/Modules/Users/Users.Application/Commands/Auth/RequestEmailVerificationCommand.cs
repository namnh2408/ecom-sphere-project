using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.DTOs;

namespace Users.Application.Commands.Auth;

/// <summary>
/// Command to request email verification
/// </summary>
public record RequestEmailVerificationCommand(
    Guid UserId
) : IRequest<Result<EmailVerificationDto>>;