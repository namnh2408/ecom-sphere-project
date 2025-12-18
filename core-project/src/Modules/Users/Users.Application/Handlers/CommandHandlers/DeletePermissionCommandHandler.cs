using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Permissions;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

public class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, Result<Unit>>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePermissionCommandHandler(IPermissionRepository permissionRepository, IUnitOfWork unitOfWork)
    {
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeletePermissionCommand command, CancellationToken cancellationToken)
    {
        await _permissionRepository.DeleteAsync(command.PermissionId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
