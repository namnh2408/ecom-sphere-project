namespace BuildingBlocks.Abstractions;

/// <summary>
/// Unit of Work pattern for managing database transactions across multiple repositories
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this unit of work to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Generic repository interface for domain entities
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TId">The entity identifier type</typeparam>
public interface IRepository<TEntity, TId> 
    where TEntity : Entity<TId>
    where TId : struct
{
    /// <summary>
    /// Gets entity by identifier
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities with pagination
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Removes an entity
    /// </summary>
    void Remove(TEntity entity);

    /// <summary>
    /// Removes entity by id
    /// </summary>
    Task<bool> RemoveAsync(TId id, CancellationToken cancellationToken = default);
}