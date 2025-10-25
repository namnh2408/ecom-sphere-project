using Microsoft.EntityFrameworkCore;
using ShopHub.Domain.Abstractions;
using ShopHub.Services.UserService.Domain.Entities;
using ShopHub.Services.UserService.Domain.Repositories;

namespace ShopHub.Services.UserService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository cho User (EF Core implementation)
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(UserDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching user by ID: {UserId}", id);
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        _logger.LogInformation("Fetching user by email: {Email}", email);
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower(), cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        _logger.LogInformation("Checking if email exists: {Email}", email);
        return await _context.Users.AnyAsync(u => u.Email == email.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all users");
        return await _context.Users.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _logger.LogInformation("Adding user: {UserId}", entity.Id);
        await _context.Users.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        _logger.LogInformation("Updating user: {UserId}", user.Id);
        
        // Check if user exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);
        if (existingUser == null)
            throw new InvalidOperationException($"User with ID {user.Id} not found");

        // Update properties
        _context.Entry(existingUser).CurrentValues.SetValues(user);
        _context.Users.Update(existingUser);
    }

    public async Task DeleteAsync(User entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _logger.LogInformation("Soft deleting user: {UserId}", entity.Id);
        
        // Soft delete - just mark as inactive
        entity.Deactivate();
        _context.Users.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeletePermanentlyAsync(User entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _logger.LogInformation("Permanently deleting user: {UserId}", entity.Id);
        _context.Users.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking if user exists: {UserId}", id);
        return await _context.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }

    public IUnitOfWork UnitOfWork => throw new NotImplementedException();
}