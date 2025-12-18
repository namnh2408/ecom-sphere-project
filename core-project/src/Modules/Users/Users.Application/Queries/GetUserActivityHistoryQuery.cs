using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.DTOs;

namespace Users.Application.Queries;

public record GetUserActivityHistoryQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 10,
    int DaysBack = 30) : IRequest<Result<PaginatedResult<UserActivityHistoryDto>>>;