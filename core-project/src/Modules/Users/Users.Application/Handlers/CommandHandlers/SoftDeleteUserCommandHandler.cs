using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Users;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

public class SoftDeleteUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SoftDeleteUserCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(SoftDeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return Result<Unit>.Fail("USER_NOT_FOUND", "User not found");
        }

        if (user.IsDeleted)
        {
            return Result<Unit>.Fail("USER_ALREADY_DELETED", "User is already deleted");
        }

        user.SoftDelete();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<Unit>.Success(Unit.Value);
    }
}
