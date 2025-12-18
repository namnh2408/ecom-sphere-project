# Controller Implementation Example

This document shows how to use custom exceptions and error handling in your controllers.

## Example 1: Basic Usage in AuthController

### Current Implementation (Before)

```csharp
[HttpPost("register")]
[AllowAnonymous]
[ProducesResponseType(typeof(AuthTokenDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
{
    var result = await mediator.Send(command);

    if (result.IsFailure)
    {
        return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
    }

    return Created($"/api/users/{result.Value!.UserId}", result.Value);
}
```

### Improved Implementation (After)

**Option 1: Using Exception Throwing (Recommended)**

```csharp
using Gateway.WebApi.Exceptions;

[HttpPost("register")]
[AllowAnonymous]
[ProducesResponseType(typeof(AuthTokenDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
{
    // Middleware will catch any exception and format the response
    var result = await mediator.Send(command);

    if (result.IsFailure)
    {
        // Throw exception instead of building response manually
        throw new BusinessRuleException(
            errorCode: result.Error!.Code,
            message: result.Error.Message,
            statusCode: StatusCodes.Status400BadRequest
        );
    }

    return Created($"/api/users/{result.Value!.UserId}", result.Value);
}
```

**Option 2: Using Helper Extension Methods**

```csharp
using Gateway.WebApi.Helpers;

[HttpPost("register")]
[AllowAnonymous]
[ProducesResponseType(typeof(AuthTokenDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
{
    var result = await mediator.Send(command);

    if (result.IsFailure)
    {
        return this.BusinessRuleError(
            errorCode: result.Error!.Code,
            message: result.Error.Message
        );
    }

    return this.Created(result.Value!, $"/api/users/{result.Value!.UserId}");
}
```

---

## Example 2: Enhanced Login Endpoint

### With Input Validation

```csharp
using Gateway.WebApi.Exceptions;

[HttpPost("login")]
[AllowAnonymous]
[ProducesResponseType(typeof(AuthTokenDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
{
    // Input validation
    if (string.IsNullOrWhiteSpace(command.Email))
    {
        throw new ValidationException("EMAIL_REQUIRED", "Email is required");
    }

    if (string.IsNullOrWhiteSpace(command.Password))
    {
        throw new ValidationException("PASSWORD_REQUIRED", "Password is required");
    }

    // Execute business logic
    var result = await mediator.Send(command);

    if (result.IsFailure)
    {
        // Handle specific error codes
        if (result.Error!.Code == "INVALID_CREDENTIALS")
        {
            throw new UnauthorizedException(result.Error.Code, result.Error.Message);
        }

        if (result.Error.Code == "ACCOUNT_LOCKED")
        {
            throw new BusinessRuleException(
                errorCode: "ACCOUNT_LOCKED",
                message: result.Error.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }

        throw new BusinessRuleException(result.Error.Code, result.Error.Message);
    }

    return Ok(result.Value);
}
```

---

## Example 3: Change Password with Complex Validation

```csharp
using Gateway.WebApi.Exceptions;
using Gateway.WebApi.Helpers;

[Authorize]
[HttpPost("change-password")]
[ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
{
    // Input validation with error details
    var validationErrors = new Dictionary<string, object>();

    if (string.IsNullOrWhiteSpace(command.CurrentPassword))
    {
        validationErrors["currentPassword"] = "Current password is required";
    }

    if (string.IsNullOrWhiteSpace(command.NewPassword))
    {
        validationErrors["newPassword"] = "New password is required";
    }

    if (command.NewPassword?.Length < 8)
    {
        validationErrors["newPassword"] = "Password must be at least 8 characters";
    }

    if (validationErrors.Count > 0)
    {
        throw new ValidationException(
            "PASSWORD_VALIDATION_FAILED",
            "Please fix validation errors",
            validationErrors
        );
    }

    // Execute command
    var result = await mediator.Send(command);

    if (result.IsFailure)
    {
        if (result.Error!.Code == "INVALID_CURRENT_PASSWORD")
        {
            throw new UnauthorizedException(result.Error.Code, result.Error.Message);
        }

        throw new BusinessRuleException(result.Error.Code, result.Error.Message);
    }

    return Ok(new { message = "Password changed successfully" });
}
```

---

## Example 4: Resource Endpoints with Full Error Handling

```csharp
using Gateway.WebApi.Exceptions;

[ApiController]
[Route("api/[controller]")]
[Tags("Users")]
public class UsersController(IMediator mediator, IUserService userService) : ControllerBase
{
    // GET: Retrieve user (with not found handling)
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await userService.GetUserAsync(id);

        // Throw if not found
        if (user == null)
        {
            throw new ResourceNotFoundException("User", id);
        }

        return Ok(user);
    }

    // POST: Create user (with duplicate handling)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException("EMAIL_REQUIRED", "Email is required");
        }

        // Check duplicate
        var existing = await userService.GetUserByEmailAsync(request.Email);
        if (existing != null)
        {
            throw new DuplicateResourceException("User", "email", request.Email);
        }

        // Create
        var user = await userService.CreateUserAsync(request);
        return Created($"/api/users/{user.Id}", user);
    }

    // PUT: Update user
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        // Get user - throw if not found
        var user = ApiResponseHelper.ThrowNotFoundIfNull(
            await userService.GetUserAsync(id),
            "User",
            id
        );

        // Authorization check
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (user.Id != currentUserId && !User.IsInRole("Admin"))
        {
            throw new ForbiddenException(
                "CANNOT_UPDATE_OTHER_USER",
                "You can only update your own profile"
            );
        }

        // Validation
        var errors = new Dictionary<string, object>();
        
        if (!IsValidEmail(request.Email))
        {
            errors["email"] = "Invalid email format";
        }

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            errors["firstName"] = "First name is required";
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(
                "UPDATE_VALIDATION_FAILED",
                "Please fix validation errors",
                errors
            );
        }

        // Update
        await userService.UpdateUserAsync(id, request);
        return Ok(new { message = "User updated successfully" });
    }

    // DELETE: Delete user (with business rule checks)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = ApiResponseHelper.ThrowNotFoundIfNull(
            await userService.GetUserAsync(id),
            "User",
            id
        );

        // Business rule: cannot delete superadmin
        if (user.Role == "SuperAdmin")
        {
            throw new BusinessRuleException(
                errorCode: "CANNOT_DELETE_SUPERADMIN",
                message: "SuperAdmin users cannot be deleted",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        // Business rule: check for active orders
        var activeOrders = await userService.GetActiveOrdersAsync(id);
        if (activeOrders.Count > 0)
        {
            throw new BusinessRuleException(
                errorCode: "USER_HAS_ACTIVE_ORDERS",
                message: "Cannot delete user with active orders",
                statusCode: StatusCodes.Status400BadRequest,
                details: new Dictionary<string, object>
                {
                    { "activeOrderCount", activeOrders.Count },
                    { "orderIds", activeOrders.Select(o => o.Id).ToList() }
                }
            );
        }

        await userService.DeleteUserAsync(id);
        return NoContent();
    }
}
```

---

## Response Examples

### Example 1: Validation Error Response

**Request:**
```bash
POST /api/auth/login
Content-Type: application/json

{ "email": "", "password": "" }
```

**Response (400 Bad Request):**
```json
{
  "error": "EMAIL_REQUIRED",
  "message": "Email is required"
}
```

---

### Example 2: Multiple Validation Errors Response

**Request:**
```bash
POST /api/users
Content-Type: application/json

{
  "email": "invalid-email",
  "firstName": "",
  "age": 15
}
```

**Response (400 Bad Request):**
```json
{
  "error": "UPDATE_VALIDATION_FAILED",
  "message": "Please fix validation errors",
  "details": {
    "email": "Invalid email format",
    "firstName": "First name is required",
    "age": "User must be at least 18 years old"
  }
}
```

---

### Example 3: Resource Not Found Response

**Request:**
```bash
GET /api/users/999
Authorization: Bearer <token>
```

**Response (404 Not Found):**
```json
{
  "error": "RESOURCE_NOT_FOUND",
  "message": "User with ID '999' not found"
}
```

---

### Example 4: Duplicate Resource Response

**Request:**
```bash
POST /api/users
Authorization: Bearer <admin-token>
Content-Type: application/json

{ "email": "existing@example.com", "firstName": "John" }
```

**Response (409 Conflict):**
```json
{
  "error": "RESOURCE_ALREADY_EXISTS",
  "message": "User with email 'existing@example.com' already exists"
}
```

---

### Example 5: Business Rule Violation Response

**Request:**
```bash
DELETE /api/users/john
Authorization: Bearer <admin-token>
```

**Response (400 Bad Request):**
```json
{
  "error": "USER_HAS_ACTIVE_ORDERS",
  "message": "Cannot delete user with active orders",
  "details": {
    "activeOrderCount": 3,
    "orderIds": ["order-1", "order-2", "order-3"]
  }
}
```

---

### Example 6: Authorization Error Response

**Request:**
```bash
PUT /api/users/other-user-id
Authorization: Bearer <user-token>
Content-Type: application/json

{ "email": "newemail@example.com", "firstName": "Jane" }
```

**Response (403 Forbidden):**
```json
{
  "error": "CANNOT_UPDATE_OTHER_USER",
  "message": "You can only update your own profile"
}
```

---

### Example 7: Authentication Error Response

**Request:**
```bash
POST /api/auth/login
Content-Type: application/json

{ "email": "user@example.com", "password": "wrongpassword" }
```

**Response (401 Unauthorized):**
```json
{
  "error": "INVALID_CREDENTIALS",
  "message": "Email or password is incorrect"
}
```

---

## Key Differences

| Aspect | Before | After |
|--------|--------|-------|
| **Error Handling** | Manual in each controller | Centralized in middleware |
| **Response Format** | Inconsistent | Always consistent |
| **Code Amount** | Many lines per controller | Fewer lines |
| **Error Throwing** | Not used | Primary method |
| **Flexibility** | Limited | Error details included |
| **Development Debug** | Limited | Full stack traces |
| **Production Safe** | Need to manage | Automatic |

---

## Step-by-Step Migration

1. **Add using statements** at top of controller:
   ```csharp
   using Gateway.WebApi.Exceptions;
   using Gateway.WebApi.Helpers;
   ```

2. **Replace BadRequest/NotFound returns** with throws:
   ```csharp
   // Before
   return BadRequest(new { error = "CODE", message = "Message" });
   
   // After
   throw new ValidationException("CODE", "Message");
   ```

3. **Remove try-catch** blocks (middleware handles them)

4. **Test** each endpoint to ensure errors work correctly

---

## Best Practices

✅ **DO:**
- Throw exceptions from business logic
- Use specific exception types
- Include helpful error messages
- Add error details for complex scenarios
- Use consistent error codes

❌ **DON'T:**
- Try-catch and return BadRequest
- Mix exception and response approaches
- Use generic Exception class
- Ignore validation errors
- Log exceptions manually (middleware does it)

---

## Questions?

See `CUSTOM_ERRORS_GUIDE.md` for more examples and patterns!