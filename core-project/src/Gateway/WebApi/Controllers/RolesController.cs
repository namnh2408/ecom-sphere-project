using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gateway.WebApi.Models;
using Users.Application.Commands.Roles;
using Users.Application.DTOs;
using Users.Application.Queries;

namespace Gateway.WebApi.Controllers;

/// <summary>
/// Role management operations including role creation, permissions assignment, and user role mapping
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Role Management")]
[Produces("application/json")]
[Consumes("application/json")]
public class RolesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get all available roles
    /// </summary>
    /// <remarks>
    /// Retrieves a list of all roles defined in the system.
    /// 
    /// **Information Included:**
    /// - Role ID and name
    /// - Role description
    /// - Creation date
    /// - Associated permissions count
    /// - Number of users with this role
    /// 
    /// **Use Cases:**
    /// - Display role options when assigning roles to users
    /// - Administrative dashboard
    /// - Role selection in UI
    /// </remarks>
    /// <returns>List of all roles</returns>
    /// <response code="200">Roles retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllRolesQuery();
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get role by unique ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information about a specific role including all permissions.
    /// 
    /// **Information Included:**
    /// - Role metadata (ID, name, description)
    /// - All permissions assigned to this role
    /// - User count with this role
    /// - Creation and modification dates
    /// </remarks>
    /// <param name="roleId">Role's unique ID</param>
    /// <returns>Role details</returns>
    /// <response code="200">Role found and retrieved</response>
    /// <response code="404">Role not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{roleId:guid}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid roleId)
    {
        var query = new GetRoleByIdQuery(roleId);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    /// <remarks>
    /// Creates a new role that can be assigned to users.
    /// 
    /// **Role Creation Rules:**
    /// - Role name must be unique
    /// - Role name is required
    /// - Description is optional
    /// - Initial permissions can be assigned separately
    /// 
    /// **After Creation:**
    /// - Role is immediately available for assignment
    /// - Permissions must be added separately using /assign-permission
    /// - Users can be assigned this role
    /// 
    /// **Role Name Conventions:**
    /// - Use PascalCase (e.g., "Administrator", "ContentEditor")
    /// - Avoid spaces and special characters
    /// </remarks>
    /// <param name="command">Role name and description</param>
    /// <returns>Created role</returns>
    /// <response code="201">Role created successfully</response>
    /// <response code="400">Invalid input or role name already exists</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateRoleCommand command)
    {
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Created($"/api/roles/{result.Value!.Id}", result.Value);
    }

    /// <summary>
    /// Update role information
    /// </summary>
    /// <remarks>
    /// Updates role metadata such as name and description.
    /// 
    /// **Updateable Fields:**
    /// - Role name
    /// - Role description
    /// 
    /// **Not Updateable:**
    /// - Role ID (immutable)
    /// - Assigned permissions (use separate endpoints)
    /// 
    /// **Impact of Changes:**
    /// - Changes apply immediately
    /// - All users with this role see the updated role info
    /// - Does not affect assigned permissions
    /// 
    /// **Important Notes:**
    /// - New role name must be unique
    /// - Cannot rename to name of existing role
    /// </remarks>
    /// <param name="roleId">Role's unique ID</param>
    /// <param name="command">Updated role information</param>
    /// <returns>Updated role</returns>
    /// <response code="200">Role updated successfully</response>
    /// <response code="404">Role not found</response>
    /// <response code="400">Invalid input or duplicate name</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut("{roleId:guid}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid roleId, [FromBody] UpdateRoleCommand command)
    {
        var updatedCommand = command with { RoleId = roleId };
        var result = await mediator.Send(updatedCommand);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Assign permission to role
    /// </summary>
    /// <remarks>
    /// Grants a specific permission to a role, automatically available to all users with that role.
    /// 
    /// **Permission Assignment Rules:**
    /// - Permission must exist in system
    /// - Cannot assign same permission twice to same role
    /// - Role must exist
    /// 
    /// **Impact on Users:**
    /// - Users with this role immediately inherit the permission
    /// - Permission is effective on next API authorization check
    /// - Users do not need to re-login
    /// 
    /// **Permission Hierarchy:**
    /// - Permissions at role level
    /// - Users inherit all permissions from all assigned roles
    /// - Multiple roles = union of all permissions
    /// 
    /// **Use Cases:**
    /// - Grant new capability to all role members
    /// - Configure role capabilities
    /// - Adjust access control
    /// </remarks>
    /// <param name="roleId">Role's unique ID</param>
    /// <param name="permissionId">Permission's unique ID to assign</param>
    /// <returns>Success message</returns>
    /// <response code="200">Permission assigned successfully</response>
    /// <response code="404">Role or permission not found</response>
    /// <response code="400">Permission already assigned or validation error</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("{roleId:guid}/permissions/{permissionId:guid}")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AssignPermission(Guid roleId, Guid permissionId)
    {
        var command = new AssignPermissionToRoleCommand(roleId, permissionId);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Permission assigned successfully" });
    }
}
