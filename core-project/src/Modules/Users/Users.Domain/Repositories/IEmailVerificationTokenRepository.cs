using Users.Domain.Entities;

namespace Users.Domain.Repositories;

public interface IEmailVerificationTokenRepository
{
    Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken = default);
    Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<EmailVerificationToken?> GetPendingByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(EmailVerificationToken token, CancellationToken cancellationToken = default);
}