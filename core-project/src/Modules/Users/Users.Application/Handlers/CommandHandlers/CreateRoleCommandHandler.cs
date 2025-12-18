using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Roles;
using Users.Application.DTOs;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

/// <summary>
/// Handler for CreateRoleCommand
/// </summary>
public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RoleDto>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        // Check if role already exists
        var existingRole = await _roleRepository.ExistsByNameAsync(command.Name, cancellationToken);
        if (existingRole)
        {
            return Result<RoleDto>.Fail("ROLE_ALREADY_EXISTS", $"Role with name '{command.Name}' already exists");
        }

        var role = Role.Create(command.Name, command.Description);
        await _roleRepository.AddAsync(role, cancellationToken);
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
