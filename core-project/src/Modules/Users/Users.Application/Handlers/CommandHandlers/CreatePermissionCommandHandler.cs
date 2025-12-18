using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Permissions;
using Users.Application.DTOs;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

/// <summary>
/// Handler for CreatePermissionCommand
/// </summary>
public class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, Result<PermissionDto>>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePermissionCommandHandler(IPermissionRepository permissionRepository, IUnitOfWork unitOfWork)
    {
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PermissionDto>> Handle(CreatePermissionCommand command, CancellationToken cancellationToken)
    {
        // Check if permission already exists
        var existingPermission = await _permissionRepository.ExistsByResourceAndActionAsync(
            command.Resource, command.Action, cancellationToken);
        if (existingPermission)
        {
            return Result<PermissionDto>.Fail(
                "PERMISSION_ALREADY_EXISTS",
                $"Permission with resource '{command.Resource}' and action '{command.Action}' already exists");
        }

        var permission = Permission.Create(command.Name, command.Description, command.Resource, command.Action);
        await _permissionRepository.AddAsync(permission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var permissionDto = new PermissionDto(
            permission.Id,
            permission.Name,
            permission.Description,
            permission.Resource,
            permission.Action,
            permission.IsActive,
            permission.CreatedAtUtc
        );

        return Result<PermissionDto>.Success(permissionDto);
    }
}
