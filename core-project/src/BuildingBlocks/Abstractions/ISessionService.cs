namespace BuildingBlocks.Abstractions;

/// <summary>
/// Session management service using Redis
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Creates or updates a session
    /// </summary>
    Task<string> CreateSessionAsync(Dictionary<string, object> data, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets session data
    /// </summary>
    Task<Dictionary<string, object>?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates session data
    /// </summary>
    Task UpdateSessionAsync(string sessionId, Dictionary<string, object> data, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a session
    /// </summary>
    Task RemoveSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a specific value in session
    /// </summary>
    Task SetSessionValueAsync(string sessionId, string key, object value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific value from session
    /// </summary>
    Task<T?> GetSessionValueAsync<T>(string sessionId, string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Extends session expiration
    /// </summary>
    Task ExtendSessionAsync(string sessionId, TimeSpan expiration, CancellationToken cancellationToken = default);
}