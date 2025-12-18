using BuildingBlocks.Abstractions;
using MediatR;

namespace Users.Application.Commands.Users;

public record DeleteProfilePictureCommand(Guid UserId) : IRequest<Result<Unit>>;