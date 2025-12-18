using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Infrastructure.Shared;

/// <summary>
/// System implementation of IClock
/// </summary>
public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}