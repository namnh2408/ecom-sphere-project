using System.Data;

namespace BuildingBlocks.Infrastructure.Shared.Dapper;

/// <summary>
/// Provides database connections for Dapper ORM
/// </summary>
public interface IDapperConnectionProvider
{
    /// <summary>
    /// Creates a new database connection (not opened)
    /// </summary>
    IDbConnection CreateConnection();

    /// <summary>
    /// Creates and opens a new database connection
    /// </summary>
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}