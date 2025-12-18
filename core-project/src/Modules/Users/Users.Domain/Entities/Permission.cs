using BuildingBlocks.Abstractions;

namespace Users.Domain.Entities;

/// <summary>
/// Permission entity - represents a specific permission/action
/// </summary>
public class Permission : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Resource { get; private set; } = null!;
    public string Action { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Permission() { }

    private Permission(Guid id, string name, string description, string resource, string action)
        : base(id)
    {
        Name = name;
        Description = description;
        Resource = resource;
        Action = action;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Permission Create(string name, string description, string resource, string action)
    {
        return new Permission(Guid.NewGuid(), name, description, resource, action);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public override string ToString() => $"{Resource}:{Action}";
}