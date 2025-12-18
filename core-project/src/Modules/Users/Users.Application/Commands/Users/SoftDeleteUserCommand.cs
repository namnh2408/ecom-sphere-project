using BuildingBlocks.Abstractions;
using MediatR;

namespace Users.Application.Commands.Users;

public record SoftDeleteUserCommand(Guid UserId) : IRequest<Result<Unit>>;