# Unit Test Documentation

## Overview

This document describes the comprehensive unit test suite for the Core Project, covering all layers: Domain, Application, and Presentation (Controllers).

## Test Project Structure

```
tests/Identity.UnitTests/
├── Domain/
│   └── UserDomainTests.cs                    # Domain entity tests
├── Handlers/
│   ├── Commands/
│   │   └── UpdateUserProfileCommandHandlerTests.cs  # Command handler tests
│   └── Queries/
│       └── GetUserByIdQueryHandlerTests.cs         # Query handler tests
├── Controllers/
│   ├── UsersControllerTests.cs               # Users API endpoint tests
│   └── AuthControllerTests.cs                # Authentication API endpoint tests
└── TEST_DOCUMENTATION.md                     # This file
```

## Test Coverage

### 1. Domain Layer Tests (`Domain/UserDomainTests.cs`)

Tests the `User` aggregate root and its business logic.

#### Test Categories:

**User Creation Tests**
- `Create_WithValidData_ReturnsSuccessResult` - Creates user with valid parameters
- `Create_WithInvalidEmail_ReturnsFailure` - Validates email format
- `Create_WithWeakPassword_ReturnsFailure` - Validates password strength
- `Create_WithEmptyFirstName_ReturnsFailure` - Validates first name
- `Create_WithEmptyLastName_ReturnsFailure` - Validates last name
- `Create_WithFirstNameTooLong_ReturnsFailure` - Enforces max length (100 chars)
- `Create_WithLastNameTooLong_ReturnsFailure` - Enforces max length (100 chars)

**Profile Update Tests**
- `UpdateProfile_WithValidNames_UpdatesSuccessfully` - Updates user profile
- `UpdateProfile_WithWhitespace_TrimsNames` - Trims whitespace from names
- `UpdateProfile_UpdatesModificationTime` - Tracks update timestamp

**Email Verification Tests**
- `VerifyEmail_WithUnverifiedUser_MarksAsVerified` - Marks email as verified

**Activation Tests**
- `Deactivate_WithActiveUser_DeactivatesSuccessfully` - Deactivates user
- `Activate_WithDeactivatedUser_ActivatesSuccessfully` - Activates user
- `Deactivate_UpdatesModificationTime` - Updates modification timestamp

**Role Management Tests**
- `AssignRole_WithNewRole_AddsRoleToUser` - Assigns single role
- `AssignRole_WithDuplicateRole_DoesNotAddDuplicate` - Prevents duplicate roles
- `AssignRole_WithMultipleRoles_AddsAllRoles` - Assigns multiple roles
- `RemoveRole_WithExistingRole_RemovesRoleSuccessfully` - Removes role
- `RemoveRole_WithNonExistentRole_DoesNotThrow` - Handles non-existent role
- `HasRole_WithAssignedRole_ReturnsTrue` - Checks role assignment
- `HasRole_WithoutAssignedRole_ReturnsFalse` - Returns false for unassigned role

**Soft Delete Tests**
- `SoftDelete_WithActiveUser_MarksAsDeleted` - Soft deletes user
- `Restore_WithDeletedUser_RestoresSuccessfully` - Restores deleted user

**Password Management Tests**
- `ChangePassword_WithCorrectOldPassword_ChangesSuccessfully` - Changes password
- `ChangePassword_WithIncorrectOldPassword_ReturnsFail` - Validates old password
- `ResetPassword_WithNewPassword_ResetsSuccessfully` - Resets password

**Profile Picture Tests**
- `SetProfilePicture_WithValidPath_SetsSuccessfully` - Sets profile picture
- `RemoveProfilePicture_WithExistingPicture_RemovesSuccessfully` - Removes picture

**Login Recording Tests**
- `RecordLogin_UpdatesLastLoginTime` - Updates last login timestamp

### 2. Application Layer - Query Handler Tests (`Handlers/Queries/GetUserByIdQueryHandlerTests.cs`)

Tests the `GetUserByIdQuery` handler for retrieving user data.

#### Test Categories:

**Success Cases**
- `Handle_WithValidUserId_ReturnsUserDto` - Retrieves user successfully
- `Handle_WithVerifiedUser_IncludesVerificationStatus` - Returns verified status
- `Handle_WithInactiveUser_IncludesInactiveStatus` - Returns inactive status
- `Handle_WithUserRoles_IncludesRoleIds` - Returns assigned roles

**Error Cases**
- `Handle_WithNonExistentUserId_ReturnsFailure` - Returns error for missing user
- `Handle_WithMultipleRequests_CallsRepositoryEachTime` - Handles multiple queries

**Integration Cases**
- `Handle_WithCancellationToken_PassesItToRepository` - Passes cancellation token

### 3. Application Layer - Command Handler Tests (`Handlers/Commands/UpdateUserProfileCommandHandlerTests.cs`)

Tests the `UpdateUserProfileCommand` handler for updating user information.

#### Test Categories:

**Success Cases**
- `Handle_WithValidCommand_UpdatesUserProfile` - Updates user profile
- `Handle_WithValidCommand_CallsRepositoryUpdate` - Calls repository update
- `Handle_WithValidCommand_CallsUnitOfWorkSaveChanges` - Saves changes via unit of work
- `Handle_WithWhitespaceNames_TrimsThemCorrectly` - Trims whitespace
- `Handle_WithMultipleUpdates_EachUpdateCallsRepository` - Handles multiple updates

**Error Cases**
- `Handle_WithNonExistentUserId_ReturnsFailure` - Returns error for missing user
- `Handle_WithNonExistentUser_DoesNotCallUpdateAsync` - Doesn't update missing user
- `Handle_WithRepositoryException_PropagatesException` - Propagates repository errors

**Integration Cases**
- `Handle_WithCancellationToken_PassesItToRepository` - Passes cancellation token

### 4. Presentation Layer - Users Controller Tests (`Controllers/UsersControllerTests.cs`)

Tests the Users API endpoints.

#### Test Categories:

**GetAll Endpoint**
- `GetAll_WithValidParameters_ReturnsOkResult` - Returns paginated users
- `GetAll_WithPagination_PassesParametersToMediator` - Passes pagination parameters
- `GetAll_WithSearchTerm_PassesSearchToMediator` - Passes search term
- `GetAll_WithQueryFailure_ReturnsBadRequest` - Returns error on failure

**GetById Endpoint**
- `GetById_WithValidUserId_ReturnsOkResult` - Returns user by ID
- `GetById_WithValidUserId_PassesUserIdToMediator` - Passes ID to handler
- `GetById_WithNonExistentUser_ReturnsNotFound` - Returns 404 for missing user

**GetByEmail Endpoint**
- `GetByEmail_WithValidEmail_ReturnsOkResult` - Returns user by email
- `GetByEmail_WithInvalidEmail_ReturnsNotFound` - Returns 404 for non-existent email

**UpdateProfile Endpoint**
- `UpdateProfile_WithValidCommand_ReturnsOkResult` - Updates profile
- `UpdateProfile_MergesUserIdFromRoute` - Merges route ID with command
- `UpdateProfile_WithNonExistentUser_ReturnsBadRequest` - Returns error for missing user

**DeactivateUser Endpoint**
- `DeactivateUser_WithValidUserId_ReturnsOkResult` - Deactivates user
- `DeactivateUser_WithNonExistentUser_ReturnsBadRequest` - Returns error for missing user

**ActivateUser Endpoint**
- `ActivateUser_WithValidUserId_ReturnsOkResult` - Activates user

**AssignRole Endpoint**
- `AssignRole_WithValidParameters_ReturnsOkResult` - Assigns role successfully
- `AssignRole_PassesParametersToMediator` - Passes parameters to handler

**RemoveRole Endpoint**
- `RemoveRole_WithValidParameters_ReturnsOkResult` - Removes role successfully
- `RemoveRole_WithError_ReturnsBadRequest` - Returns error on failure

### 5. Presentation Layer - Auth Controller Tests (`Controllers/AuthControllerTests.cs`)

Tests the Authentication API endpoints.

#### Test Categories:

**Register Endpoint**
- `Register_WithValidCommand_ReturnsCreatedResult` - Registers user successfully
- `Register_WithValidCommand_ReturnsLocationHeader` - Returns location header
- `Register_WithInvalidEmail_ReturnsBadRequest` - Validates email format
- `Register_WithWeakPassword_ReturnsBadRequest` - Validates password strength
- `Register_WithExistingEmail_ReturnsBadRequest` - Prevents duplicate emails

**Login Endpoint**
- `Login_WithValidCredentials_ReturnsOkResult` - Logs in successfully
- `Login_WithInvalidCredentials_ReturnsUnauthorized` - Returns 401 for wrong password
- `Login_WithNonExistentUser_ReturnsUnauthorized` - Returns 401 for missing user
- `Login_WithInactiveUser_ReturnsUnauthorized` - Returns 401 for inactive user
- `Login_WithLockedAccount_ReturnsUnauthorized` - Returns 401 for locked account
- `Login_SendsCommandToMediator` - Sends command to handler

**RefreshToken Endpoint**
- `RefreshToken_WithValidToken_ReturnsOkResult` - Refreshes token successfully
- `RefreshToken_WithInvalidToken_ReturnsUnauthorized` - Returns 401 for invalid token

**ChangePassword Endpoint**
- `ChangePassword_WithValidCommand_ReturnsOkResult` - Changes password successfully
- `ChangePassword_WithIncorrectOldPassword_ReturnsBadRequest` - Validates old password
- `ChangePassword_WithWeakNewPassword_ReturnsBadRequest` - Validates new password

**RequestPasswordReset Endpoint**
- `RequestPasswordReset_WithValidEmail_ReturnsOkResult` - Sends reset email
- `RequestPasswordReset_WithNonExistentEmail_ReturnsOkResult` - Returns OK for security

**ResetPassword Endpoint**
- `ResetPassword_WithValidToken_ReturnsOkResult` - Resets password successfully
- `ResetPassword_WithInvalidToken_ReturnsBadRequest` - Returns error for invalid token

**VerifyEmail Endpoint**
- `VerifyEmail_WithValidToken_ReturnsOkResult` - Verifies email successfully
- `VerifyEmail_WithInvalidToken_ReturnsBadRequest` - Returns error for invalid token

## Testing Tools & Libraries

- **xUnit**: Test framework
- **FluentAssertions**: Assertion library for readable assertions
- **Moq**: Mocking library for creating test doubles
- **Bogus**: Data generator for test data
- **Microsoft.AspNetCore.Mvc.Testing**: Testing support for ASP.NET Core

## Shared Testing Utilities

Located in `tests/Shared.Testing/`:

### UserDataBuilder (`Builders/UserDataBuilder.cs`)
Builder pattern for creating test user DTOs with fluent API.

```csharp
var user = UserDataBuilder.CreateDefault()
    .WithEmail("test@example.com")
    .WithFirstName("Jane")
    .Build();
```

### UserRepositoryFixture (`Fixtures/UserRepositoryFixture.cs`)
Fixture for setting up mock repositories with predefined behaviors.

```csharp
var fixture = new UserRepositoryFixture();
fixture.SetupGetByIdAsync(userId, testUser);
var mock = fixture.UserRepositoryMock;
```

## Running Tests

### All Tests
```powershell
dotnet test d:\projects\core-project\tests\Identity.UnitTests
```

### Specific Test Class
```powershell
dotnet test d:\projects\core-project\tests\Identity.UnitTests --filter "FullyQualifiedName~UserDomainTests"
```

### Specific Test
```powershell
dotnet test d:\projects\core-project\tests\Identity.UnitTests --filter "Name=Create_WithValidData_ReturnsSuccessResult"
```

### With Coverage Report
```powershell
dotnet test d:\projects\core-project\tests\Identity.UnitTests /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Watch Mode (Run tests on file changes)
```powershell
dotnet watch --project d:\projects\core-project\tests\Identity.UnitTests test
```

## Test Naming Convention

All tests follow the AAA (Arrange-Act-Assert) pattern and use descriptive names:

```
[MethodName]_[Scenario]_[Expected Result]
```

Example: `Create_WithInvalidEmail_ReturnsFailure`

## Test Statistics

- **Total Test Classes**: 5
- **Total Test Methods**: 100+
- **Coverage Areas**:
  - Domain Layer: ~35 tests
  - Application Layer (Queries): ~10 tests
  - Application Layer (Commands): ~15 tests
  - Presentation Layer: ~45+ tests

## Best Practices Implemented

1. **Isolation**: Each test is independent and can run in any order
2. **Mocking**: External dependencies are mocked to test in isolation
3. **Naming**: Descriptive names clearly indicate what is being tested
4. **Arrangement**: Test setup is clear and organized
5. **Single Responsibility**: Each test validates one behavior
6. **No Side Effects**: Tests don't depend on execution order or shared state
7. **Fast Execution**: Tests run quickly (no database, file system, or network)
8. **Comprehensive Coverage**: Both happy path and error cases are tested

## Adding New Tests

When adding new tests, follow these guidelines:

1. Create a new test class in the appropriate folder
2. Name the class `[FeatureName]Tests`
3. Follow the AAA pattern (Arrange, Act, Assert)
4. Use FluentAssertions for readable assertions
5. Group related tests with `#region` blocks
6. Add XML comments for complex test logic
7. Use builders and fixtures for common test data setup

## Continuous Integration

These tests are designed to run in CI/CD pipelines. They:
- Don't require external services
- Don't modify any state
- Run deterministically
- Execute quickly
- Produce clear failure messages

## Future Enhancements

- Add integration tests for database operations
- Add API endpoint tests using WebApplicationFactory
- Add performance benchmarks
- Add mutation testing
- Add contract tests for APIs