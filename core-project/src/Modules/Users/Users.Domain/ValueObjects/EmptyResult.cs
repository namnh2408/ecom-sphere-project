namespace Users.Domain.ValueObjects;

/// <summary>
/// Represents an empty result for domain operations that don't return a value
/// </summary>
public sealed class EmptyResult
{
    public static readonly EmptyResult Instance = new();
}