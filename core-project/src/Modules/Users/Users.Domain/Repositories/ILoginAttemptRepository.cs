using Users.Domain.Entities;

namespace Users.Domain.Repositories;

public interface ILoginAttemptRepository
{
    Task AddAsync(LoginAttempt attempt, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LoginAttempt>> GetRecentFailedAttemptsAsync(Guid userId, int minutesBack = 15, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LoginAttempt>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LoginAttempt>> GetUserLoginAttemptsAsync(Guid userId, int? limit = null, int daysBack = 30, CancellationToken cancellationToken = default);
}