using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.UserRoles;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

/// <summary>
/// Handler for AssignRoleToUserCommand
/// </summary>
public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand, Result<Unit>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignRoleToUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(AssignRoleToUserCommand command, CancellationToken cancellationToken)
    {
        // Get user
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result<Unit>.Fail("USER_NOT_FOUND", $"User with ID '{command.UserId}' was not found");
        }

        // Get role
        var role = await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken);
        if (role is null)
        {
            return Result<Unit>.Fail("ROLE_NOT_FOUND", $"Role with ID '{command.RoleId}' was not found");
        }

        // Assign role
        user.AssignRole(command.RoleId, role.Name);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
