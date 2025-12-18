using Users.Domain.Entities;

namespace Users.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}