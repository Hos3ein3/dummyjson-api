using DummyJson.Domain.Common.Interfaces;
using SharedKernel.Results;
using DummyJson.Domain.Common.Primitives;
using DummyJson.Domain.Users.Events;
using Microsoft.AspNetCore.Identity;

namespace DummyJson.Domain.Users;

/// <summary>
/// User aggregate root — extended by ASP.NET Identity's ApplicationUser in Infrastructure.
/// Domain-level user data lives here; Identity concerns (password hashing etc.) are in Infrastructure.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>, IEntity<Guid>, IAuditable, ISoftDelete
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    public uint Version { get; private set; } // Optimistic locking

    private ApplicationUser() { }

    private ApplicationUser(
        Guid id,
        string firstName,
        string lastName,
        string userName,
        string email,
        string phoneNumber,
        string? image,
        string? gender,
        DateOnly? birthDate)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        UserName = userName;
        Email = email;
        PhoneNumber = phoneNumber;
        Image = image;
        Gender = gender;
        BirthDate = birthDate;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Image { get; private set; }
    public string? Gender { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public string? Role { get; private set; } = "user";

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

    public static Result<ApplicationUser> Create(
        string firstName,
        string lastName,
        string userName,
        string email,
        string phoneNumber,
        string? image = null,
        string? gender = null,
        DateOnly? birthDate = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<ApplicationUser>(Error.Validation(nameof(email), "Email cannot be empty."));

        if (string.IsNullOrWhiteSpace(userName))
            return Result.Failure<ApplicationUser>(Error.Validation(nameof(userName), "Username cannot be empty."));

        var user = new ApplicationUser(IdGenerator.NewId(), firstName, lastName, userName, email, phoneNumber, image, gender, birthDate);
        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, user.Email));
        return Result.Success(user);
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    public Result UpdateProfile(string firstName, string lastName, string phoneNumber, string? image)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Image = image;
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
