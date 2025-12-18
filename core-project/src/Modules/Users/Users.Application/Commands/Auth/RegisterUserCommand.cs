using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.DTOs;

namespace Users.Application.Commands.Auth;

public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName
) : IRequest<Result<AuthTokenDto>>;