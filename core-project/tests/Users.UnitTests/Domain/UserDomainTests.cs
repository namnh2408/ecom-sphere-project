using FluentAssertions;
using Users.Domain.Entities;

namespace Identity.UnitTests.Domain;

public class UserDomainTests
{
    #region User Creation Tests

    [Fact]
    public void Create_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Test@123456";
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var result = User.Create(email, password, firstName, lastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Value.Should().Be(email);
        result.Value.FirstName.Should().Be(firstName);
        result.Value.LastName.Should().Be(lastName);
        result.Value.IsActive.Should().BeTrue();
        result.Value.IsEmailVerified.Should().BeFalse();
    }

    [Fact]
    public void Create_WithInvalidEmail_ReturnsFailure()
    {
        // Arrange
        var invalidEmail = "not-an-email";
        var password = "Test@123456";
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var result = User.Create(invalidEmail, password, firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithWeakPassword_ReturnsFailure()
    {
        // Arrange
        var email = "test@example.com";
        var weakPassword = "weak";
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var result = User.Create(email, weakPassword, firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithEmptyFirstName_ReturnsFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Test@123456";
        var firstName = "";
        var lastName = "Doe";

        // Act
        var result = User.Create(email, password, firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("FIRSTNAME_INVALID");
    }

    [Fact]
    public void Create_WithEmptyLastName_ReturnsFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Test@123456";
        var firstName = "John";
        var lastName = "";

        // Act
        var result = User.Create(email, password, firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("LASTNAME_INVALID");
    }

    [Fact]
    public void Create_WithFirstNameTooLong_ReturnsFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Test@123456";
        var firstName = new string('a', 101);
        var lastName = "Doe";

        // Act
        var result = User.Create(email, password, firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("FIRSTNAME_INVALID");
    }

    [Fact]
    public void Create_WithLastNameTooLong_ReturnsFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Test@123456";
        var firstName = "John";
        var lastName = new string('a', 101);

        // Act
        var result = User.Create(email, password, firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("LASTNAME_INVALID");
    }

    #endregion

    #region Profile Update Tests

    [Fact]
    public void UpdateProfile_WithValidNames_UpdatesSuccessfully()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        var newFirstName = "Jane";
        var newLastName = "Smith";

        // Act
        user.UpdateProfile(newFirstName, newLastName);

        // Assert
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
    }

    [Fact]
    public void UpdateProfile_WithWhitespace_TrimsNames()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        var newFirstName = "  Jane  ";
        var newLastName = "  Smith  ";

        // Act
        user.UpdateProfile(newFirstName, newLastName);

        // Assert
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
    }

    [Fact]
    public void UpdateProfile_UpdatesModificationTime()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        var originalUpdatedAtUtc = user.UpdatedAtUtc;

        // Act
        System.Threading.Thread.Sleep(10);
        user.UpdateProfile("Jane", "Smith");

        // Assert
        user.UpdatedAtUtc.Should().BeAfter(originalUpdatedAtUtc ?? DateTime.MinValue);
    }

    #endregion

    #region Email Verification Tests

    [Fact]
    public void VerifyEmail_WithUnverifiedUser_MarksAsVerified()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        user.IsEmailVerified.Should().BeFalse();

        // Act
        user.VerifyEmail();

        // Assert
        user.IsEmailVerified.Should().BeTrue();
    }

    #endregion

    #region Activation Tests

    [Fact]
    public void Deactivate_WithActiveUser_DeactivatesSuccessfully()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        user.IsActive.Should().BeTrue();

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_WithDeactivatedUser_ActivatesSuccessfully()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        user.Deactivate();
        user.IsActive.Should().BeFalse();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_UpdatesModificationTime()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        var originalUpdatedAtUtc = user.UpdatedAtUtc;

        // Act
        System.Threading.Thread.Sleep(10);
        user.Deactivate();

        // Assert
        user.UpdatedAtUtc.Should().BeAfter(originalUpdatedAtUtc ?? DateTime.MinValue);
    }

    #endregion

    #region Role Management Tests

    [Fact]
    public void AssignRole_WithNewRole_AddsRoleToUser()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        var roleId = Guid.NewGuid();
        user.RoleIds.Should().BeEmpty();

        // Act
        user.AssignRole(roleId, "Admin");

        // Assert
        user.RoleIds.Should().Contain(roleId);
        user.RoleIds.Should().HaveCount(1);
    }

    [Fact]
    public void AssignRole_WithDuplicateRole_DoesNotAddDuplicate()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        var roleId = Guid.NewGuid();

        // Act
        user.AssignRole(roleId, "Admin");
        user.AssignRole(roleId, "Admin");

        // Assert
        user.RoleIds.Should().HaveCount(1);
    }

    [Fact]
    public void AssignRole_WithMultipleRoles_AddsAllRoles()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        var roleId1 = Guid.NewGuid();
        var roleId2 = Guid.NewGuid();
        var roleId3 = Guid.NewGuid();

        // Act
        user.AssignRole(roleId1, "Admin");
        user.AssignRole(roleId2, "Editor");
        user.AssignRole(roleId3, "Viewer");

        // Assert
        user.RoleIds.Should().HaveCount(3);
        user.RoleIds.Should().Contain(new[] { roleId1, roleId2, roleId3 });
    }

    [Fact]
    public void RemoveRole_WithExistingRole_RemovesRoleSuccessfully()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        var roleId = Guid.NewGuid();
        user.AssignRole(roleId, "Admin");

        // Act
        user.RemoveRole(roleId, "Admin");

        // Assert
        user.RoleIds.Should().NotContain(roleId);
        user.RoleIds.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRole_WithNonExistentRole_DoesNotThrow()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        var roleId = Guid.NewGuid();

        // Act & Assert
        user.RemoveRole(roleId, "Admin");
        user.RoleIds.Should().BeEmpty();
    }

    [Fact]
    public void HasRole_WithAssignedRole_ReturnsTrue()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        var roleId = Guid.NewGuid();
        user.AssignRole(roleId, "Admin");

        // Act
        var hasRole = user.HasRole(roleId);

        // Assert
        hasRole.Should().BeTrue();
    }

    [Fact]
    public void HasRole_WithoutAssignedRole_ReturnsFalse()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        var roleId = Guid.NewGuid();

        // Act
        var hasRole = user.HasRole(roleId);

        // Assert
        hasRole.Should().BeFalse();
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void SoftDelete_WithActiveUser_MarksAsDeleted()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        user.IsDeleted.Should().BeFalse();

        // Act
        user.SoftDelete();

        // Assert
        user.IsDeleted.Should().BeTrue();
        user.DeletedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Restore_WithDeletedUser_RestoresSuccessfully()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        user.SoftDelete();

        // Act
        user.Restore();

        // Assert
        user.IsDeleted.Should().BeFalse();
        user.DeletedAtUtc.Should().BeNull();
    }

    #endregion

    #region Password Management Tests

    [Fact]
    public void ChangePassword_WithCorrectOldPassword_ChangesSuccessfully()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        var newPassword = "NewTest@123456";

        // Act
        var changeResult = user.ChangePassword("Test@123456", newPassword);

        // Assert
        changeResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ChangePassword_WithIncorrectOldPassword_ReturnsFail()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;

        // Act
        var changeResult = user.ChangePassword("WrongPassword", "NewTest@123456");

        // Assert
        changeResult.IsFailure.Should().BeTrue();
        changeResult.Error!.Code.Should().Be("PASSWORD_INCORRECT");
    }

    [Fact]
    public void ResetPassword_WithNewPassword_ResetsSuccessfully()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        var newPassword = "NewTest@123456";

        // Act
        var resetResult = user.ResetPassword(newPassword);

        // Assert
        resetResult.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Profile Picture Tests

    [Fact]
    public void SetProfilePicture_WithValidPath_SetsSuccessfully()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        var picturePath = "/images/profile.jpg";

        // Act
        user.SetProfilePicture(picturePath);

        // Assert
        user.ProfilePicturePath.Should().Be(picturePath);
    }

    [Fact]
    public void RemoveProfilePicture_WithExistingPicture_RemovesSuccessfully()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        user.SetProfilePicture("/images/profile.jpg");

        // Act
        user.RemoveProfilePicture();

        // Assert
        user.ProfilePicturePath.Should().BeNull();
    }

    #endregion

    #region Login Recording Tests

    [Fact]
    public void RecordLogin_UpdatesLastLoginTime()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test@123456", "John", "Doe");
        var user = result.Value!;
        var originalLastLogin = user.LastLoginAtUtc;

        // Act
        System.Threading.Thread.Sleep(10);
        user.RecordLogin();

        // Assert
        user.LastLoginAtUtc.Should().NotBeNull();
        user.LastLoginAtUtc.Should().BeAfter(originalLastLogin ?? DateTime.MinValue);
    }

    #endregion
}