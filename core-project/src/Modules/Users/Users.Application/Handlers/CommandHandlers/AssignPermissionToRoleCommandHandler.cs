using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Roles;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

/// <summary>
/// Handler for AssignPermissionToRoleCommand
/// </summary>
public class AssignPermissionToRoleCommandHandler : IRequestHandler<AssignPermissionToRoleCommand, Result<Unit>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignPermissionToRoleCommandHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(AssignPermissionToRoleCommand command, CancellationToken cancellationToken)
    {
        // Get role
        var role = await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken);
        if (role is null)
        {
            return Result<Unit>.Fail("ROLE_NOT_FOUND", $"Role with ID '{command.RoleId}' was not found");
        }

        // Get permission
        var permission = await _permissionRepository.GetByIdAsync(command.PermissionId, cancellationToken);
        if (permission is null)
        {
            return Result<Unit>.Fail("PERMISSION_NOT_FOUND", $"Permission with ID '{command.PermissionId}' was not found");
        }

        // Assign permission
        role.AddPermission(command.PermissionId);
        await _roleRepository.UpdateAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
