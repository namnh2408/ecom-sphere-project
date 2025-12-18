using Users.Domain.Entities;

namespace Users.Domain.Repositories;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task UpdateAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
}