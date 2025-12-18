using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Roles;
using Users.Application.DTOs;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<RoleDto>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleCommandHandler(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RoleDto>> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken);
        if (role is null)
        {
            return Result<RoleDto>.Fail("ROLE_NOT_FOUND", $"Role with ID '{command.RoleId}' was not found");
        }

        role.Update(command.Name, command.Description);
        await _roleRepository.UpdateAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roleDto = new RoleDto(
            role.Id,
            role.Name,
            role.Description,
            role.IsActive,
            role.PermissionIds,
            role.CreatedAtUtc,
            role.UpdatedAtUtc
        );

        return Result<RoleDto>.Success(roleDto);
    }
}
