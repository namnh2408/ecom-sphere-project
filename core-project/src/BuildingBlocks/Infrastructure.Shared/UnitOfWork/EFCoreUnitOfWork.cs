using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Infrastructure.Shared.UnitOfWork;

/// <summary>
/// Generic Unit of Work implementation for EF Core DbContext
/// Provides transaction and change management for all modules
/// </summary>
/// <typeparam name="TDbContext">The DbContext type</typeparam>
public class EFCoreUnitOfWork<TDbContext> : IUnitOfWork
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public EFCoreUnitOfWork(TDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Saves all changes made in this unit of work to the database
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.CommitTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.RollbackTransactionAsync(cancellationToken);
    }
}