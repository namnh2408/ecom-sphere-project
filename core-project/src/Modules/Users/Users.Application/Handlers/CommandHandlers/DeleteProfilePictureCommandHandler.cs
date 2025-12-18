using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Users;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

public class DeleteProfilePictureCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteProfilePictureCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return Result<Unit>.Fail("USER_NOT_FOUND", "User not found");
        }

        if (string.IsNullOrEmpty(user.ProfilePicturePath))
        {
            return Result<Unit>.Fail("NO_PROFILE_PICTURE", "User has no profile picture");
        }

        // Here you would delete the file from disk or cloud storage
        // For now, we'll just remove the path reference

        user.RemoveProfilePicture();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
