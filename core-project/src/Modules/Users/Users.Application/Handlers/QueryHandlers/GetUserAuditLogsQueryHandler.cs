using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.DTOs;
using Users.Application.Queries;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.QueryHandlers;

public class GetUserAuditLogsQueryHandler(
    IAuditLogRepository auditLogRepository) : IRequestHandler<GetUserAuditLogsQuery, Result<PaginatedResult<AuditLogDto>>>
{
    public async Task<Result<PaginatedResult<AuditLogDto>>> Handle(
        GetUserAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var auditLogs = await auditLogRepository.GetAuditLogsAsync(request.UserId, daysBack: request.DaysBack);

            var totalCount = auditLogs.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var items = auditLogs
                .OrderByDescending(x => x.OccurredAtUtc)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new AuditLogDto(
                    x.Id,
                    x.UserId,
                    x.EntityName,
                    x.EntityId,
                    x.OperationType,
                    x.OldValues,
                    x.NewValues,
                    x.Description,
                    x.IpAddress,
                    x.OccurredAtUtc))
                .ToList();

            var result = new PaginatedResult<AuditLogDto>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize,
                totalPages,
                request.PageNumber > 1,
                request.PageNumber < totalPages);

            return Result<PaginatedResult<AuditLogDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PaginatedResult<AuditLogDto>>.Fail("ERROR", $"An error occurred: {ex.Message}");
        }
    }
}