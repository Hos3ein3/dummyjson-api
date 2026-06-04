using FluentValidation;

namespace DummyJson.Application.Auth.Commands;

// ── Registration DTOs ────────────────────────────────────────────────────────
public sealed record RegisterByEmailRequest(string Email, string Password, string FirstName, string LastName);
public sealed record RegisterByUsernameRequest(string Username, string Password, string FirstName, string LastName);
public sealed record RegisterByPhoneNumberRequest(string PhoneNumber, string Password, string FirstName, string LastName);

// ── Login DTOs ───────────────────────────────────────────────────────────────
public sealed record LoginByEmailRequest(string Email, string Password);
public sealed record LoginByUsernameRequest(string Username, string Password);
public sealed record LoginByPhoneNumberRequest(string PhoneNumber, string Password);

// ── Google Auth DTO ──────────────────────────────────────────────────────────
public sealed record GoogleAuthRequest(string GoogleIdToken, string? FirstName = null, string? LastName = null);

// ── Shared DTOs ──────────────────────────────────────────────────────────────
public sealed record RefreshTokenRequest(string RefreshToken);

// ── Validators ───────────────────────────────────────────────────────────────

public class RegisterByEmailRequestValidator : AbstractValidator<RegisterByEmailRequest>
{
    public RegisterByEmailRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
    }
}

public class RegisterByUsernameRequestValidator : AbstractValidator<RegisterByUsernameRequest>
{
    public RegisterByUsernameRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(30);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
    }
}

public class RegisterByPhoneNumberRequestValidator : AbstractValidator<RegisterByPhoneNumberRequest>
{
    public RegisterByPhoneNumberRequestValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
    }
}

public class LoginByEmailRequestValidator : AbstractValidator<LoginByEmailRequest>
{
    public LoginByEmailRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginByUsernameRequestValidator : AbstractValidator<LoginByUsernameRequest>
{
    public LoginByUsernameRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginByPhoneNumberRequestValidator : AbstractValidator<LoginByPhoneNumberRequest>
{
    public LoginByPhoneNumberRequestValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class GoogleAuthRequestValidator : AbstractValidator<GoogleAuthRequest>
{
    public GoogleAuthRequestValidator()
    {
        RuleFor(x => x.GoogleIdToken).NotEmpty();
    }
}

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
