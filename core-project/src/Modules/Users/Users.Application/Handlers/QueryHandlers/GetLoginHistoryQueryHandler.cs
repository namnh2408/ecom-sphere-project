using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.DTOs;
using Users.Application.Queries;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.QueryHandlers;

public class GetLoginHistoryQueryHandler(
    ILoginAttemptRepository loginAttemptRepository) : IRequestHandler<GetLoginHistoryQuery, Result<PaginatedResult<LoginHistoryDto>>>
{
    public async Task<Result<PaginatedResult<LoginHistoryDto>>> Handle(
        GetLoginHistoryQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var attempts = await loginAttemptRepository.GetUserLoginAttemptsAsync(request.UserId, daysBack: request.DaysBack);

            var totalCount = attempts.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var items = attempts
                .OrderByDescending(x => x.AttemptedAtUtc)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new LoginHistoryDto(
                    x.Id,
                    x.Email,
                    x.IsSuccessful,
                    x.FailureReason,
                    x.IpAddress,
                    x.UserAgent,
                    x.AttemptedAtUtc))
                .ToList();

            var result = new PaginatedResult<LoginHistoryDto>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize,
                totalPages,
                request.PageNumber > 1,
                request.PageNumber < totalPages);

            return Result<PaginatedResult<LoginHistoryDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PaginatedResult<LoginHistoryDto>>.Fail("ERROR", $"An error occurred: {ex.Message}");
        }
    }
}