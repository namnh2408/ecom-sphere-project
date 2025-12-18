using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Roles;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result<Unit>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleCommandHandler(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        // Verify role exists before deletion
        var role = await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken);
        if (role is null)
        {
            return Result<Unit>.Fail("ROLE_NOT_FOUND", $"Role with ID '{command.RoleId}' not found");
        }

        await _roleRepository.DeleteAsync(command.RoleId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
