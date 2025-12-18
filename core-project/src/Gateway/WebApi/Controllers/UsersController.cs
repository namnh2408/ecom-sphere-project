using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using Gateway.WebApi.Models;
using Users.Application.Commands.Users;
using Users.Application.Commands.UserRoles;
using Users.Application.DTOs;
using Users.Application.Queries;

namespace Gateway.WebApi.Controllers;

/// <summary>
/// User management operations including profile management, role assignment, and history tracking
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("User Management")]
[Produces("application/json")]
[Consumes("application/json")]
public class UsersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get all users with pagination and filtering options
    /// </summary>
    /// <remarks>
    /// Retrieves a paginated list of users with optional filtering and search capabilities.
    /// 
    /// **Filter Options:**
    /// - **searchTerm**: Search by user name or email (partial match)
    /// - **isActive**: Filter by account status (active/inactive)
    /// - **isEmailVerified**: Filter by email verification status
    /// 
    /// **Pagination:**
    /// - Default page size: 10 users
    /// - Supports custom page size up to 100
    /// 
    /// **Response Format:**
    /// Returns paginated results with metadata (totalCount, totalPages, currentPage)
    /// </remarks>
    /// <param name="pageNumber">Page number (1-based indexing)</param>
    /// <param name="pageSize">Number of records per page (1-100)</param>
    /// <param name="searchTerm">Search by name or email</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="isEmailVerified">Filter by email verification status</param>
    /// <returns>Paginated list of users</returns>
    /// <response code="200">Users retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isEmailVerified = null)
    {
        var query = new GetAllUsersQuery(pageNumber, pageSize, searchTerm, isActive, isEmailVerified);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get user by unique ID
    /// </summary>
    /// <remarks>
    /// Retrieves complete user information by their unique identifier.
    /// 
    /// **Includes:**
    /// - User profile (email, name, created date)
    /// - Account status (active/inactive, verified)
    /// - Associated roles and permissions
    /// - Profile picture URL (if exists)
    /// </remarks>
    /// <param name="userId">User's unique ID (GUID)</param>
    /// <returns>Complete user information</returns>
    /// <response code="200">User found and retrieved</response>
    /// <response code="404">User not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid userId)
    {
        var query = new GetUserByIdQuery(userId);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get user by email address
    /// </summary>
    /// <remarks>
    /// Retrieves complete user information by their email address.
    /// 
    /// **Use Cases:**
    /// - Look up user by email for admin purposes
    /// - Verify user existence during onboarding
    /// - Find user for support tickets
    /// </remarks>
    /// <param name="email">User's email address</param>
    /// <returns>Complete user information</returns>
    /// <response code="200">User found and retrieved</response>
    /// <response code="404">User not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var query = new GetUserByEmailQuery(email);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Update user profile information
    /// </summary>
    /// <remarks>
    /// Updates editable user profile fields such as name, phone, and other contact information.
    /// 
    /// **Editable Fields:**
    /// - First Name
    /// - Last Name
    /// - Phone Number
    /// - Company/Organization
    /// - Address/Location
    /// 
    /// **Not Editable:**
    /// - Email (requires separate change email flow)
    /// - User ID
    /// - Created Date
    /// - Account Status
    /// 
    /// **Audit Trail:**
    /// - All changes are recorded in audit logs
    /// - Change history includes before/after values
    /// </remarks>
    /// <param name="userId">User's unique ID</param>
    /// <param name="command">Updated profile information</param>
    /// <returns>Updated user data</returns>
    /// <response code="200">Profile updated successfully</response>
    /// <response code="404">User not found</response>
    /// <response code="400">Invalid input data</response>
    [HttpPut("{userId:guid}/profile")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile(Guid userId, [FromBody] UpdateUserProfileCommand command)
    {
        var updatedCommand = command with { UserId = userId };
        var result = await mediator.Send(updatedCommand);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Assign role to user
    /// </summary>
    /// <remarks>
    /// Grants a specific role to a user, providing them with associated permissions.
    /// 
    /// **Role Assignment Rules:**
    /// - User can have multiple roles
    /// - Cannot assign the same role twice
    /// - Role must exist in the system
    /// - User must be active
    /// 
    /// **Permission Inheritance:**
    /// - Role assignments are effective immediately
    /// - Permissions are inherited from the role
    /// - Changes take effect on next API call
    /// </remarks>
    /// <param name="userId">User's unique ID</param>
    /// <param name="roleId">Role's unique ID to assign</param>
    /// <returns>Success message</returns>
    /// <response code="200">Role assigned successfully</response>
    /// <response code="404">User or role not found</response>
    /// <response code="400">Role already assigned or validation error</response>
    [HttpPost("{userId:guid}/roles/{roleId:guid}")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignRole(Guid userId, Guid roleId)
    {
        var command = new AssignRoleToUserCommand(userId, roleId);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Role assigned successfully" });
    }

    /// <summary>
    /// Remove role from user
    /// </summary>
    /// <remarks>
    /// Revokes a specific role from a user, removing associated permissions.
    /// 
    /// **Removal Rules:**
    /// - Role must be currently assigned to user
    /// - At least one role must remain (cannot remove all roles)
    /// - Changes take effect immediately
    /// 
    /// **Impact:**
    /// - User loses permissions from this role
    /// - If user is using a permission from this role, access is revoked
    /// - Changes are audited
    /// </remarks>
    /// <param name="userId">User's unique ID</param>
    /// <param name="roleId">Role's unique ID to remove</param>
    /// <returns>Success message</returns>
    /// <response code="200">Role removed successfully</response>
    /// <response code="404">User or role not found</response>
    /// <response code="400">Role not assigned or validation error</response>
    [HttpDelete("{userId:guid}/roles/{roleId:guid}")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId)
    {
        var command = new RemoveRoleFromUserCommand(userId, roleId);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Role removed successfully" });
    }

    /// <summary>
    /// Deactivate user account
    /// </summary>
    /// <remarks>
    /// Temporarily disables a user account, preventing login and API access.
    /// 
    /// **Deactivation Features:**
    /// - User cannot login
    /// - Existing JWT tokens become invalid
    /// - All API access is blocked
    /// - Account data is preserved
    /// - Can be reactivated at any time
    /// 
    /// **Use Cases:**
    /// - Temporary access suspension
    /// - Account maintenance by user
    /// - Administrative action (e.g., policy violation)
    /// - Optional: suspension during investigation
    /// 
    /// **Difference from Soft Delete:**
    /// - Deactivated accounts can be reactivated
    /// - Soft deleted accounts are permanently marked as deleted
    /// - Audit logs are maintained
    /// </remarks>
    /// <param name="userId">User's unique ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">User deactivated successfully</response>
    /// <response code="404">User not found</response>
    /// <response code="400">User already deactivated</response>
    [HttpPost("{userId:guid}/deactivate")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeactivateUser(Guid userId)
    {
        var command = new DeactivateUserCommand(userId);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "User deactivated successfully" });
    }

    /// <summary>
    /// Activate user account
    /// </summary>
    /// <remarks>
    /// Re-enables a previously deactivated user account, allowing login and API access.
    /// 
    /// **Activation Features:**
    /// - User can login again
    /// - Access is restored immediately
    /// - Previous tokens remain invalid (user must re-login)
    /// - Account history is preserved
    /// - Audit trail updated
    /// 
    /// **Prerequisites:**
    /// - Account must be currently deactivated
    /// - Account must not be soft-deleted
    /// </remarks>
    /// <param name="userId">User's unique ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">User activated successfully</response>
    /// <response code="404">User not found</response>
    /// <response code="400">User already active</response>
    [HttpPost("{userId:guid}/activate")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActivateUser(Guid userId)
    {
        var command = new ActivateUserCommand(userId);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "User activated successfully" });
    }

    /// <summary>
    /// Soft delete user (reversible delete)
    /// </summary>
    /// <remarks>
    /// Marks a user account as deleted while preserving all data for compliance and audit purposes.
    /// 
    /// **Soft Delete Features:**
    /// - Account is marked as deleted
    /// - Data is retained for audit/compliance (GDPR retention)
    /// - User cannot login
    /// - Account cannot be recovered through normal means
    /// - Requires admin action for restoration
    /// 
    /// **Data Retention:**
    /// - All user data, transactions, and audit logs are preserved
    /// - Stored for legal/compliance retention periods
    /// - Can be permanently deleted after retention period
    /// 
    /// **Difference from Deactivation:**
    /// - Soft deleted accounts are considered permanently deleted
    /// - Data is kept for compliance
    /// - Cannot be simply reactivated by user
    /// 
    /// **Security Note:**
    /// - Email address becomes available for reuse
    /// - Account will not appear in user lists
    /// </remarks>
    /// <param name="userId">User's unique ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">User deleted successfully</response>
    /// <response code="404">User not found</response>
    /// <response code="400">User already deleted</response>
    [HttpDelete("{userId:guid}")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SoftDeleteUser(Guid userId)
    {
        var command = new SoftDeleteUserCommand(userId);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "User deleted successfully" });
    }

    /// <summary>
    /// Upload user profile picture
    /// </summary>
    /// <remarks>
    /// Uploads a new profile picture for the user, replacing any existing one.
    /// 
    /// **Supported Formats:**
    /// - JPEG (.jpg, .jpeg)
    /// - PNG (.png)
    /// - GIF (.gif)
    /// - WebP (.webp)
    /// 
    /// **Size Restrictions:**
    /// - Maximum file size: 5 MB
    /// - Minimum dimensions: 100x100 pixels
    /// - Recommended dimensions: 500x500 pixels or larger
    /// 
    /// **Storage:**
    /// - Files are stored securely on server
    /// - Old pictures are automatically deleted
    /// - Picture URL is returned for display
    /// 
    /// **Security:**
    /// - Only image files allowed
    /// - MIME type validation
    /// - File content verification
    /// </remarks>
    /// <param name="userId">User's unique ID</param>
    /// <param name="file">Image file (form-data)</param>
    /// <returns>Picture path/URL</returns>
    /// <response code="200">Picture uploaded successfully, returns picture path</response>
    /// <response code="404">User not found</response>
    /// <response code="400">Invalid file format, size exceeded, or no file provided</response>
    [HttpPost("{userId:guid}/profile-picture")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(PictureUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadProfilePicture(Guid userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "NO_FILE", message = "No file provided" });
        }

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var fileContent = stream.ToArray();

        var command = new UploadProfilePictureCommand(userId, file.FileName, file.ContentType, fileContent);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Profile picture uploaded successfully", picturePath = result.Value });
    }

    /// <summary>
    /// Delete user profile picture
    /// </summary>
    /// <remarks>
    /// Removes the user's profile picture from the system.
    /// 
    /// **Behavior:**
    /// - Picture is permanently deleted
    /// - File is removed from storage
    /// - User is shown default/no picture in UI
    /// - Action is audited
    /// 
    /// **Notes:**
    /// - Cannot undo deletion (picture must be re-uploaded)
    /// - Deletion succeeds even if no picture exists
    /// </remarks>
    /// <param name="userId">User's unique ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">Picture deleted successfully (or didn't exist)</response>
    /// <response code="404">User not found</response>
    [HttpDelete("{userId:guid}/profile-picture")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProfilePicture(Guid userId)
    {
        var command = new DeleteProfilePictureCommand(userId);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Profile picture deleted successfully" });
    }

    /// <summary>
    /// Get login history for a user
    /// </summary>
    /// <remarks>
    /// Retrieves historical record of all login attempts (successful and failed) for a user.
    /// 
    /// **Features:**
    /// - Shows both successful and failed login attempts
    /// - IP address tracking
    /// - User agent (device/browser info)
    /// - Exact timestamp (UTC)
    /// - Paginated results
    /// 
    /// **Use Cases:**
    /// - Security audit: Review suspicious login activity
    /// - User: Check where they accessed their account
    /// - Admin: Monitor for unauthorized access attempts
    /// 
    /// **Default Period:**
    /// - Shows last 30 days of login history
    /// - Configurable via daysBack parameter
    /// 
    /// **Security Indicator:**
    /// - Multiple failed attempts may indicate password guessing
    /// - Logins from unusual locations should be reviewed
    /// </remarks>
    /// <param name="userId">User's unique ID</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Records per page</param>
    /// <param name="daysBack">Number of days to look back (default: 30)</param>
    /// <returns>Paginated login history</returns>
    /// <response code="200">Login history retrieved successfully</response>
    /// <response code="404">User not found</response>
    [HttpGet("{userId:guid}/login-history")]
    [ProducesResponseType(typeof(PaginatedResult<LoginHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLoginHistory(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int daysBack = 30)
    {
        var query = new GetLoginHistoryQuery(userId, pageNumber, pageSize, daysBack);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get activity history for a user
    /// </summary>
    /// <remarks>
    /// Retrieves comprehensive activity log showing all user actions performed in the system.
    /// 
    /// **Tracked Activities:**
    /// - LOGIN: User login event
    /// - LOGOUT: User logout event
    /// - CREATE_ROLE: User created a role
    /// - UPDATE_PROFILE: User profile was updated
    /// - DELETE_ACCOUNT: Account deletion
    /// - UPLOAD_PICTURE: Profile picture uploaded
    /// - CHANGE_PASSWORD: Password changed
    /// - And more...
    /// 
    /// **Captured Details:**
    /// - Activity type (enum)
    /// - IP address
    /// - User agent (browser/device)
    /// - Timestamp (UTC)
    /// - Metadata (JSON with activity-specific info)
    /// 
    /// **Use Cases:**
    /// - Audit trail for compliance (SOC 2, GDPR)
    /// - Troubleshooting user issues
    /// - Security investigation
    /// - User behavior analysis
    /// </remarks>
    /// <param name="userId">User's unique ID</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Records per page</param>
    /// <param name="daysBack">Number of days to look back (default: 30)</param>
    /// <returns>Paginated activity history</returns>
    /// <response code="200">Activity history retrieved successfully</response>
    /// <response code="404">User not found</response>
    [HttpGet("{userId:guid}/activity-history")]
    [ProducesResponseType(typeof(PaginatedResult<UserActivityHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActivityHistory(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int daysBack = 30)
    {
        var query = new GetUserActivityHistoryQuery(userId, pageNumber, pageSize, daysBack);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get audit logs for a user
    /// </summary>
    /// <remarks>
    /// Retrieves detailed audit logs of all changes made to user records and related entities.
    /// 
    /// **Tracked Operations:**
    /// - CREATE: New entity created
    /// - UPDATE: Entity fields modified
    /// - DELETE: Entity soft/hard deleted
    /// - RESTORE: Entity restored from soft delete
    /// 
    /// **Captured for Each Change:**
    /// - Entity name and ID
    /// - Operation type
    /// - Modified by (User ID)
    /// - Before and after values (JSON)
    /// - Change timestamp (UTC)
    /// - IP address of the actor
    /// 
    /// **Use Cases:**
    /// - Compliance auditing (GDPR, HIPAA, PCI-DSS)
    /// - Investigate unauthorized changes
    /// - Track data modifications
    /// - Historical data recovery
    /// - Regulatory reporting
    /// 
    /// **Default Period:**
    /// - Shows last 90 days of audit logs
    /// - Configurable via daysBack parameter
    /// - Older logs are archived/deleted per policy
    /// </remarks>
    /// <param name="userId">User's unique ID</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Records per page</param>
    /// <param name="daysBack">Number of days to look back (default: 90)</param>
    /// <returns>Paginated audit logs</returns>
    /// <response code="200">Audit logs retrieved successfully</response>
    /// <response code="404">User not found</response>
    [HttpGet("{userId:guid}/audit-logs")]
    [ProducesResponseType(typeof(PaginatedResult<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuditLogs(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int daysBack = 90)
    {
        var query = new GetUserAuditLogsQuery(userId, pageNumber, pageSize, daysBack);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }
}
