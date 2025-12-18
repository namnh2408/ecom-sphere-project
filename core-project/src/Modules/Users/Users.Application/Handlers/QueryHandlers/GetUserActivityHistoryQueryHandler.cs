using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.DTOs;
using Users.Application.Queries;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.QueryHandlers;

public class GetUserActivityHistoryQueryHandler(
    IUserActivityHistoryRepository activityHistoryRepository) : IRequestHandler<GetUserActivityHistoryQuery, Result<PaginatedResult<UserActivityHistoryDto>>>
{
    public async Task<Result<PaginatedResult<UserActivityHistoryDto>>> Handle(
        GetUserActivityHistoryQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var activities = await activityHistoryRepository.GetUserActivitiesAsync(request.UserId, daysBack: request.DaysBack);

            var totalCount = activities.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var items = activities
                .OrderByDescending(x => x.OccurredAtUtc)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new UserActivityHistoryDto(
                    x.Id,
                    x.UserId,
                    x.ActivityType,
                    x.Description,
                    x.IpAddress,
                    x.UserAgent,
                    x.OccurredAtUtc,
                    x.Metadata))
                .ToList();

            var result = new PaginatedResult<UserActivityHistoryDto>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize,
                totalPages,
                request.PageNumber > 1,
                request.PageNumber < totalPages);

            return Result<PaginatedResult<UserActivityHistoryDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PaginatedResult<UserActivityHistoryDto>>.Fail("ERROR", $"An error occurred: {ex.Message}");
        }
    }
}