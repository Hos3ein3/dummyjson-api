using DummyJson.Domain.Common.Interfaces;
using SharedKernel.Results;
using DummyJson.Domain.Common.Primitives;
using DummyJson.Domain.Users.Events;

namespace DummyJson.Domain.Users;

/// <summary>
/// User aggregate root — extended by ASP.NET Identity's ApplicationUser in Infrastructure.
/// Domain-level user data lives here; Identity concerns (password hashing etc.) are in Infrastructure.
/// </summary>
public sealed class User : AggregateRoot<Guid>, IAuditable, ISoftDelete
{
    private User() { }

    private User(
        Guid id,
        string firstName,
        string lastName,
        string username,
        string email,
        string phone,
        string? image,
        string? gender,
        DateOnly? birthDate) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Username = username;
        Email = email;
        Phone = phone;
        Image = image;
        Gender = gender;
        BirthDate = birthDate;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? Image { get; private set; }
    public string? Gender { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public string? Role { get; private set; } = "user";

    // Address (value object)
    public Address? Address { get; private set; }

    // IAuditable
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    // ── Factory ───────────────────────────────────────────────────────────────

    public static Result<User> Create(
        string firstName,
        string lastName,
        string username,
        string email,
        string phone,
        string? image = null,
        string? gender = null,
        DateOnly? birthDate = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<User>(Error.Validation(nameof(email), "Email cannot be empty."));

        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure<User>(Error.Validation(nameof(username), "Username cannot be empty."));

        var user = new User(Guid.CreateVersion7(), firstName, lastName, username, email, phone, image, gender, birthDate);
        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, user.Email));
        return Result.Success(user);
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    public Result UpdateProfile(string firstName, string lastName, string phone, string? image, Address? address)
    {
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        Image = image;
        Address = address;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public void AssignRole(string role)
    {
        Role = role;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Delete(string? deletedBy = null)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
