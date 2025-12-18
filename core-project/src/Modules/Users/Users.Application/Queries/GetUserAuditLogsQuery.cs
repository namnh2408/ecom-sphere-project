using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.DTOs;

namespace Users.Application.Queries;

public record GetUserAuditLogsQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 10,
    int DaysBack = 90) : IRequest<Result<PaginatedResult<AuditLogDto>>>;