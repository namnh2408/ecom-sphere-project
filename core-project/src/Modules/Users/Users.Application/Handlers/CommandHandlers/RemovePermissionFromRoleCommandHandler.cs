using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Roles;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

public class RemovePermissionFromRoleCommandHandler : IRequestHandler<RemovePermissionFromRoleCommand, Result<Unit>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemovePermissionFromRoleCommandHandler(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(RemovePermissionFromRoleCommand command, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken);
        if (role is null)
        {
            return Result<Unit>.Fail("ROLE_NOT_FOUND", $"Role with ID '{command.RoleId}' was not found");
        }

        role.RemovePermission(command.PermissionId);
        await _roleRepository.UpdateAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
