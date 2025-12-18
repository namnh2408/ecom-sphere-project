namespace BuildingBlocks.Abstractions;

/// <summary>
/// Abstraction for system time to support testability
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current UTC time
    /// </summary>
    DateTime UtcNow { get; }
}