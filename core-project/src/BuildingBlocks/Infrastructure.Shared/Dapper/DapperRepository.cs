using BuildingBlocks.Abstractions;
using Dapper;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Shared.Dapper;

/// <summary>
/// Generic Dapper repository for read-only operations
/// Provides a base implementation for querying data using Dapper ORM
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public abstract class DapperRepository<T> : IDapperRepository<T> where T : class
{
    protected readonly IDapperConnectionProvider _connectionProvider;
    protected readonly ILogger<DapperRepository<T>> _logger;

    protected DapperRepository(IDapperConnectionProvider connectionProvider, ILogger<DapperRepository<T>> logger)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResult?> QuerySingleOrDefaultAsync<TResult>(
        string sql, 
        object? parameters = null, 
        CancellationToken cancellationToken = default) 
        where TResult : class
    {
        try
        {
            _logger.LogDebug("Executing query: {Sql}", sql);
            
            using var connection = await _connectionProvider.CreateConnectionAsync(cancellationToken);
            var result = await connection.QuerySingleOrDefaultAsync<TResult>(sql, parameters);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query: {Sql}", sql);
            throw;
        }
    }

    public async Task<IEnumerable<TResult>> QueryAsync<TResult>(
        string sql, 
        object? parameters = null, 
        CancellationToken cancellationToken = default) 
        where TResult : class
    {
        try
        {
            _logger.LogDebug("Executing query: {Sql}", sql);
            
            using var connection = await _connectionProvider.CreateConnectionAsync(cancellationToken);
            var result = await connection.QueryAsync<TResult>(sql, parameters);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query: {Sql}", sql);
            throw;
        }
    }

    public async Task<object?> QueryScalarAsync(
        string sql, 
        object? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Executing scalar query: {Sql}", sql);
            
            using var connection = await _connectionProvider.CreateConnectionAsync(cancellationToken);
            var result = await connection.QueryFirstOrDefaultAsync(sql, parameters);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing scalar query: {Sql}", sql);
            throw;
        }
    }

    public async Task<TResult?> QueryScalarAsync<TResult>(
        string sql, 
        object? parameters = null, 
        CancellationToken cancellationToken = default)
        where TResult : struct
    {
        try
        {
            _logger.LogDebug("Executing scalar query: {Sql}", sql);
            
            using var connection = await _connectionProvider.CreateConnectionAsync(cancellationToken);
            var result = await connection.QueryFirstOrDefaultAsync<TResult?>(sql, parameters);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing scalar query: {Sql}", sql);
            throw;
        }
    }

    public async Task<int> ExecuteAsync(
        string sql, 
        object? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Executing command: {Sql}", sql);
            
            using var connection = await _connectionProvider.CreateConnectionAsync(cancellationToken);
            var result = await connection.ExecuteAsync(sql, parameters);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command: {Sql}", sql);
            throw;
        }
    }
}