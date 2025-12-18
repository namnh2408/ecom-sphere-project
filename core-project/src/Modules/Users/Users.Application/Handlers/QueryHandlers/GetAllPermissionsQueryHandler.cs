using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.DTOs;
using Users.Application.Queries;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.QueryHandlers;

/// <summary>
/// Handler for GetAllPermissionsQuery
/// </summary>
public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, Result<IEnumerable<PermissionDto>>>
{
    private readonly IPermissionRepository _permissionRepository;

    public GetAllPermissionsQueryHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<Result<IEnumerable<PermissionDto>>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _permissionRepository.GetAllAsync(cancellationToken);
        
        var permissionDtos = permissions.Select(p => new PermissionDto(
            p.Id,
            p.Name,
            p.Description,
            p.Resource,
            p.Action,
            p.IsActive,
            p.CreatedAtUtc
        )).ToList();

        return Result<IEnumerable<PermissionDto>>.Success(permissionDtos);
    }
}