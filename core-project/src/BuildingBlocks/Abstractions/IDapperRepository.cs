namespace BuildingBlocks.Abstractions;

/// <summary>
/// Generic Dapper Repository interface for read-only operations
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IDapperRepository<T> where T : class
{
    /// <summary>
    /// Executes a query and returns a single result as DTO
    /// </summary>
    Task<TResult?> QuerySingleOrDefaultAsync<TResult>(string sql, object? parameters = null, CancellationToken cancellationToken = default) 
        where TResult : class;

    /// <summary>
    /// Executes a query and returns multiple results as DTOs
    /// </summary>
    Task<IEnumerable<TResult>> QueryAsync<TResult>(string sql, object? parameters = null, CancellationToken cancellationToken = default) 
        where TResult : class;

    /// <summary>
    /// Executes a scalar query (COUNT, SUM, MAX, MIN, etc.)
    /// </summary>
    Task<object?> QueryScalarAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a scalar query returning a specific type (COUNT, SUM, etc.)
    /// </summary>
    Task<TResult?> QueryScalarAsync<TResult>(string sql, object? parameters = null, CancellationToken cancellationToken = default) 
        where TResult : struct;

    /// <summary>
    /// Executes a command (INSERT, UPDATE, DELETE)
    /// </summary>
    Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default);
}