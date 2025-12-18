using Bogus;
using Users.Application.DTOs;

namespace Shared.Testing.Builders;

/// <summary>
/// Builder pattern for creating test user data
/// </summary>
public class UserDataBuilder
{
    private readonly Faker _faker = new();
    private Guid _id = Guid.NewGuid();
    private string _email = "";
    private string _firstName = "";
    private string _lastName = "";
    private bool _isActive = true;
    private bool _isEmailVerified = false;
    private IReadOnlyList<Guid> _roleIds = new List<Guid>();
    private DateTime _createdAtUtc = DateTime.UtcNow;
    private DateTime? _updatedAtUtc = null;
    private DateTime? _lastLoginAtUtc = null;

    public UserDataBuilder()
    {
        _email = _faker.Internet.Email();
        _firstName = _faker.Person.FirstName;
        _lastName = _faker.Person.LastName;
    }

    public UserDataBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public UserDataBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserDataBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public UserDataBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public UserDataBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public UserDataBuilder WithIsEmailVerified(bool isEmailVerified)
    {
        _isEmailVerified = isEmailVerified;
        return this;
    }

    public UserDataBuilder WithRoleIds(params Guid[] roleIds)
    {
        _roleIds = roleIds.ToList();
        return this;
    }

    public UserDataBuilder WithCreatedAtUtc(DateTime createdAtUtc)
    {
        _createdAtUtc = createdAtUtc;
        return this;
    }

    public UserDataBuilder WithUpdatedAtUtc(DateTime? updatedAtUtc)
    {
        _updatedAtUtc = updatedAtUtc;
        return this;
    }

    public UserDataBuilder WithLastLoginAtUtc(DateTime? lastLoginAtUtc)
    {
        _lastLoginAtUtc = lastLoginAtUtc;
        return this;
    }

    public UserDto Build()
    {
        return new UserDto(
            _id,
            _email,
            _firstName,
            _lastName,
            _isActive,
            _isEmailVerified,
            _roleIds,
            _createdAtUtc,
            _updatedAtUtc,
            _lastLoginAtUtc
        );
    }

    public static UserDataBuilder CreateDefault()
    {
        return new UserDataBuilder();
    }

    public static UserDataBuilder CreateInactive()
    {
        return new UserDataBuilder().WithIsActive(false);
    }

    public static UserDataBuilder CreateVerified()
    {
        return new UserDataBuilder().WithIsEmailVerified(true);
    }
}