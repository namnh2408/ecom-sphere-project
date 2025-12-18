using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.UserRoles;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

public class RemoveRoleFromUserCommandHandler : IRequestHandler<RemoveRoleFromUserCommand, Result<Unit>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveRoleFromUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(RemoveRoleFromUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result<Unit>.Fail("USER_NOT_FOUND", $"User with ID '{command.UserId}' was not found");
        }

        var role = await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken);
        if (role is null)
        {
            return Result<Unit>.Fail("ROLE_NOT_FOUND", $"Role with ID '{command.RoleId}' was not found");
        }

        user.RemoveRole(command.RoleId, role.Name);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
