using BuildingBlocks.Abstractions;
using MediatR;

namespace Users.Application.Commands.Users;

public record UploadProfilePictureCommand(
    Guid UserId,
    string FileName,
    string ContentType,
    byte[] FileContent) : IRequest<Result<string>>;