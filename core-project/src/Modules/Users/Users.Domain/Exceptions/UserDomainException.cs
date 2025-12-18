namespace Users.Domain.Exceptions;

/// <summary>
/// Base exception for User domain
/// </summary>
public class UserDomainException : Exception
{
    public string? Code { get; }

    public UserDomainException(string message, string? code = null) : base(message)
    {
        Code = code;
    }

    public UserDomainException(string message, Exception innerException, string? code = null)
        : base(message, innerException)
    {
        Code = code;
    }
}

/// <summary>
/// Exception raised when user is not found
/// </summary>
public class UserNotFoundException : UserDomainException
{
    public Guid UserId { get; }

    public UserNotFoundException(Guid userId)
        : base($"User with ID '{userId}' was not found", "USER_NOT_FOUND")
    {
        UserId = userId;
    }
}

/// <summary>
/// Exception raised when user email already exists
/// </summary>
public class UserEmailAlreadyExistsException : UserDomainException
{
    public string Email { get; }

    public UserEmailAlreadyExistsException(string email)
        : base($"User with email '{email}' already exists", "USER_EMAIL_ALREADY_EXISTS")
    {
        Email = email;
    }
}

/// <summary>
/// Exception raised when authentication fails
/// </summary>
public class InvalidCredentialsException : UserDomainException
{
    public InvalidCredentialsException()
        : base("Invalid email or password", "INVALID_CREDENTIALS")
    {
    }
}

/// <summary>
/// Exception raised when role is not found
/// </summary>
public class RoleNotFoundException : UserDomainException
{
    public Guid RoleId { get; }

    public RoleNotFoundException(Guid roleId)
        : base($"Role with ID '{roleId}' was not found", "ROLE_NOT_FOUND")
    {
        RoleId = roleId;
    }
}

/// <summary>
/// Exception raised when role name already exists
/// </summary>
public class RoleAlreadyExistsException : UserDomainException
{
    public string RoleName { get; }

    public RoleAlreadyExistsException(string roleName)
        : base($"Role with name '{roleName}' already exists", "ROLE_ALREADY_EXISTS")
    {
        RoleName = roleName;
    }
}

/// <summary>
/// Exception raised when permission is not found
/// </summary>
public class PermissionNotFoundException : UserDomainException
{
    public Guid PermissionId { get; }

    public PermissionNotFoundException(Guid permissionId)
        : base($"Permission with ID '{permissionId}' was not found", "PERMISSION_NOT_FOUND")
    {
        PermissionId = permissionId;
    }
}