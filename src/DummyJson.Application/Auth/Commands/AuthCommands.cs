using DummyJson.Application.Common.CQRS;
using SharedKernel.Results;

namespace DummyJson.Application.Auth.Commands;

// ── Register ─────────────────────────────────────────────────────────────────

public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Username,
    string Email,
    string Password,
    string? Phone = null,
    string? Gender = null) : ICommand<Result<AuthResponse>>;

// ── Login ─────────────────────────────────────────────────────────────────────

public sealed record LoginCommand(
    string Username,
    string Password,
    bool ExpiresInMins = false) : ICommand<Result<AuthResponse>>;

// ── Refresh Token ─────────────────────────────────────────────────────────────

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<Result<AuthResponse>>;

// ── Logout ────────────────────────────────────────────────────────────────────

public sealed record LogoutCommand(string RefreshToken) : ICommand<Result>;

// ── Response ──────────────────────────────────────────────────────────────────

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiry,
    Guid UserId,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    string? Image,
    string Role);
