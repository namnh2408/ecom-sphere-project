using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Shared.Dapper;

/// <summary>
/// Provides SQL connections for Dapper ORM
/// Manages separate connection pool from EF Core
/// </summary>
public class DapperConnectionProvider : IDapperConnectionProvider
{
    private readonly string _connectionString;
    private readonly ILogger<DapperConnectionProvider> _logger;

    public DapperConnectionProvider(string connectionString, ILogger<DapperConnectionProvider> logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates and returns a new SQL connection
    /// </summary>
    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(_connectionString);
        _logger.LogDebug("Created new Dapper SqlConnection");
        return connection;
    }

    /// <summary>
    /// Creates and opens a new SQL connection
    /// </summary>
    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(_connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            _logger.LogDebug("Opened new Dapper SqlConnection");
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open Dapper SqlConnection");
            connection.Dispose();
            throw;
        }
    }
}