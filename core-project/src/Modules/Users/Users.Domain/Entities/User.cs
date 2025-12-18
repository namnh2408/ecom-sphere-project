using BuildingBlocks.Abstractions;
using Users.Domain.DomainEvents;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities;

/// <summary>
/// Result for operations that don't return a value
/// </summary>
public sealed class OperationResult
{
    public static readonly OperationResult Success = new();
}

/// <summary>
/// User aggregate root - represents an authenticated user with roles and permissions
/// </summary>
public class User : Entity<Guid>
{
    public Email Email { get; private set; } = null!;
    public Password Password { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public string? ProfilePicturePath { get; private set; }

    private readonly List<Guid> _roleIds = new();

    /// <summary>
    /// Read-only list of role IDs assigned to this user
    /// </summary>
    public IReadOnlyList<Guid> RoleIds => _roleIds.AsReadOnly();

    private User() { }

    private User(Guid id, Email email, Password password, string firstName, string lastName)
        : base(id)
    {
        Email = email;
        Password = password;
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
        IsEmailVerified = false;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new user with the provided information
    /// </summary>
    public static Result<User> Create(string email, string plainPassword, string firstName, string lastName)
    {
        // Validate email
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            return Result<User>.Fail(emailResult.Error!.Code, emailResult.Error.Message);
        }

        // Validate password
        var passwordResult = Password.Create(plainPassword);
        if (passwordResult.IsFailure)
        {
            return Result<User>.Fail(passwordResult.Error!.Code, passwordResult.Error.Message);
        }

        // Validate names
        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length > 100)
        {
            return Result<User>.Fail("FIRSTNAME_INVALID", "First name must be between 1 and 100 characters");
        }

        if (string.IsNullOrWhiteSpace(lastName) || lastName.Length > 100)
        {
            return Result<User>.Fail("LASTNAME_INVALID", "Last name must be between 1 and 100 characters");
        }

        var userId = Guid.NewGuid();
        var user = new User(userId, emailResult.Value!, passwordResult.Value!, firstName.Trim(), lastName.Trim());

        // Raise domain event
        user.Raise(new UserCreatedDomainEvent(
            userId,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            DateTime.UtcNow));

        return Result<User>.Success(user);
    }

    /// <summary>
    /// Changes the user's password
    /// </summary>
    public Result<OperationResult> ChangePassword(string oldPlainPassword, string newPlainPassword)
    {
        if (!Password.VerifyPassword(oldPlainPassword))
        {
            return Result<OperationResult>.Fail("PASSWORD_INCORRECT", "Current password is incorrect");
        }

        var passwordResult = Password.Create(newPlainPassword);
        if (passwordResult.IsFailure)
        {
            return Result<OperationResult>.Fail(passwordResult.Error!.Code, passwordResult.Error.Message);
        }

        Password = passwordResult.Value!;
        UpdatedAtUtc = DateTime.UtcNow;

        Raise(new UserPasswordChangedDomainEvent(
            Id,
            Email.Value,
            DateTime.UtcNow));

        return Result<OperationResult>.Success(OperationResult.Success);
    }

    /// <summary>
    /// Resets the password (for admin or password recovery scenarios)
    /// </summary>
    public Result<OperationResult> ResetPassword(string newPlainPassword)
    {
        var passwordResult = Password.Create(newPlainPassword);
        if (passwordResult.IsFailure)
        {
            return Result<OperationResult>.Fail(passwordResult.Error!.Code, passwordResult.Error.Message);
        }

        Password = passwordResult.Value!;
        UpdatedAtUtc = DateTime.UtcNow;

        Raise(new UserPasswordChangedDomainEvent(
            Id,
            Email.Value,
            DateTime.UtcNow));

        return Result<OperationResult>.Success(OperationResult.Success);
    }

    /// <summary>
    /// Assigns a role to this user
    /// </summary>
    public void AssignRole(Guid roleId, string roleName)
    {
        if (!_roleIds.Contains(roleId))
        {
            _roleIds.Add(roleId);
            UpdatedAtUtc = DateTime.UtcNow;

            Raise(new RoleAssignedToUserDomainEvent(
                Id,
                roleId,
                roleName,
                DateTime.UtcNow));
        }
    }

    /// <summary>
    /// Removes a role from this user
    /// </summary>
    public void RemoveRole(Guid roleId, string roleName)
    {
        if (_roleIds.Remove(roleId))
        {
            UpdatedAtUtc = DateTime.UtcNow;

            Raise(new RoleRemovedFromUserDomainEvent(
                Id,
                roleId,
                roleName,
                DateTime.UtcNow));
        }
    }

    /// <summary>
    /// Checks if the user has a specific role
    /// </summary>
    public bool HasRole(Guid roleId)
    {
        return _roleIds.Contains(roleId);
    }

    /// <summary>
    /// Updates user profile information
    /// </summary>
    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks email as verified
    /// </summary>
    public void VerifyEmail()
    {
        IsEmailVerified = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the user account
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivates the user account
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Records the last login time
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft deletes the user account
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Restores a soft-deleted user
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAtUtc = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the user's profile picture path
    /// </summary>
    public void SetProfilePicture(string picturePath)
    {
        ProfilePicturePath = picturePath;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes the user's profile picture
    /// </summary>
    public void RemoveProfilePicture()
    {
        ProfilePicturePath = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public override string ToString() => $"{FirstName} {LastName} ({Email})";
}