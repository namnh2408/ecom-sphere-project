using ShopHub.Domain.Abstractions;
using ShopHub.Services.UserService.Domain.Entities;

namespace ShopHub.Services.UserService.Domain.Repositories;

/// <summary>
/// Repository interface cho User (Commands/Writes)
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Tìm user theo email
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra email đã tồn tại
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}