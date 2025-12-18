using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.DTOs;
using Users.Application.Queries;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.QueryHandlers;

/// <summary>
/// Handler for GetAllRolesQuery
/// </summary>
public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<IEnumerable<RoleDto>>>
{
    private readonly IRoleRepository _roleRepository;

    public GetAllRolesQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<IEnumerable<RoleDto>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);
        
        var roleDtos = roles.Select(r => new RoleDto(
            r.Id,
            r.Name,
            r.Description,
            r.IsActive,
            r.PermissionIds,
            r.CreatedAtUtc,
            r.UpdatedAtUtc
        )).ToList();

        return Result<IEnumerable<RoleDto>>.Success(roleDtos);
    }
}