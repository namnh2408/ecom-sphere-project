using BuildingBlocks.Abstractions;

namespace Users.Domain.Entities;

/// <summary>
/// Role entity - represents a collection of permissions
/// </summary>
public class Role : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private readonly List<Guid> _permissionIds = new();

    /// <summary>
    /// Read-only list of permission IDs
    /// </summary>
    public IReadOnlyList<Guid> PermissionIds => _permissionIds.AsReadOnly();

    private Role() { }

    private Role(Guid id, string name, string description)
        : base(id)
    {
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Role Create(string name, string description)
    {
        return new Role(Guid.NewGuid(), name, description);
    }

    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddPermission(Guid permissionId)
    {
        if (!_permissionIds.Contains(permissionId))
        {
            _permissionIds.Add(permissionId);
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    public void RemovePermission(Guid permissionId)
    {
        if (_permissionIds.Remove(permissionId))
        {
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    public bool HasPermission(Guid permissionId)
    {
        return _permissionIds.Contains(permissionId);
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public override string ToString() => Name;
}