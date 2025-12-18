using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Users;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

public class ActivateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ActivateUserCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return Result<Unit>.Fail("USER_NOT_FOUND", "User not found");
        }

        if (user.IsActive)
        {
            return Result<Unit>.Fail("USER_ALREADY_ACTIVE", "User is already active");
        }

        user.Activate();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<Unit>.Success(Unit.Value);
    }
}
