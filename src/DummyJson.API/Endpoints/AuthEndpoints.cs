using Api.Extensions;
using Application.Common.Errors;
using Application.Common.Validation;
using DummyJson.Application.Auth.Commands;
using DummyJson.Application.Auth.Services;
using DummyJson.Application.Users.Events;
using DummyJson.Domain.Users;
using DummyJson.Persistence.Context;
using DummyJson.Persistence.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using SharedKernel.Results;
using System.Threading;
using System.Threading.Tasks;

namespace DummyJson.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        bool IsWebClient(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Client-Type", out var type) && type == "web")
                return true;

            var userAgent = context.Request.Headers.UserAgent.ToString().ToLowerInvariant();
            return userAgent.Contains("mozilla") || userAgent.Contains("chrome") || userAgent.Contains("safari");
        }

        void AppendTokensToCookies(HttpContext context, string accessToken, string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = System.DateTimeOffset.UtcNow.AddDays(7)
            };
            context.Response.Cookies.Append("AccessToken", accessToken, cookieOptions);
            context.Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);
        }

        async Task<IResult> HandleAuthSuccess(
            ApplicationUser user,
            HttpContext context,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            DummyJson.Infrastructure.Events.IntegrationEventDispatcher eventDispatcher,
            ApplicationUserManager userManager,
            bool isRegistration,
            CancellationToken ct)
        {
            var roles = await userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "user";

            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.UserName!, user.Email ?? user.UserName!, role, fullName, user.PhoneNumber);
            var refreshToken = jwtTokenService.GenerateRefreshToken();
            await refreshTokenService.StoreRefreshTokenAsync(user.Id, refreshToken, ct);

            // Dispatch integration event only on registration
            if (isRegistration)
            {
                await eventDispatcher.DispatchAsync(new UserRegisteredIntegrationEvent(user.Id, user.Email), ct);
            }

            // Map to response DTO
            var response = new AuthResponse(
                IsWebClient(context) ? "" : accessToken, 
                IsWebClient(context) ? "" : refreshToken,
                jwtTokenService.GetAccessTokenExpiry(),
                user.Id, user.UserName!, user.Email ?? "",
                user.FirstName, user.LastName, user.Image, role);

            if (isRegistration)
            {
                return role.Equals("admin", StringComparison.OrdinalIgnoreCase) 
                ? Results.Created($"/api/v1/users/{user.Id}", response) 
                : Results.Created($"/api/v1/users/me", response);
            }

            if (IsWebClient(context))
            {
                AppendTokensToCookies(context, accessToken, refreshToken);
            }

            return Results.Ok(response);
        }

        // ── Registration Endpoints ──────────────────────────────────────────────────

        group.MapPost("/register/email", async (
            RegisterByEmailRequest request,
            IValidator<RegisterByEmailRequest> validator,
            IErrorFactory errorFactory,
            ApplicationUserManager userManager,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            DummyJson.Infrastructure.Events.IntegrationEventDispatcher eventDispatcher,
            HttpContext context,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateToResultAsync(request, errorFactory, ct);
            if (validationResult.IsFailure) return validationResult.ToIResult(context);

            var result = await userManager.RegisterByEmailAsync(request.Email, request.Password, request.FirstName, request.LastName);
            if (result.IsFailure) return result.ToIResult(context);

            return await HandleAuthSuccess(result.Value, context, jwtTokenService, refreshTokenService, eventDispatcher, userManager, true, ct);
        });

        group.MapPost("/register/username", async (
            RegisterByUsernameRequest request,
            IValidator<RegisterByUsernameRequest> validator,
            IErrorFactory errorFactory,
            ApplicationUserManager userManager,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            DummyJson.Infrastructure.Events.IntegrationEventDispatcher eventDispatcher,
            HttpContext context,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateToResultAsync(request, errorFactory, ct);
            if (validationResult.IsFailure) return validationResult.ToIResult(context);

            var result = await userManager.RegisterByUsernameAsync(request.Username, request.Password, request.FirstName, request.LastName);
            if (result.IsFailure) return result.ToIResult(context);

            return await HandleAuthSuccess(result.Value, context, jwtTokenService, refreshTokenService, eventDispatcher, userManager, true, ct);
        });

        group.MapPost("/register/phone", async (
            RegisterByPhoneNumberRequest request,
            IValidator<RegisterByPhoneNumberRequest> validator,
            IErrorFactory errorFactory,
            ApplicationUserManager userManager,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            DummyJson.Infrastructure.Events.IntegrationEventDispatcher eventDispatcher,
            HttpContext context,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateToResultAsync(request, errorFactory, ct);
            if (validationResult.IsFailure) return validationResult.ToIResult(context);

            var result = await userManager.RegisterByPhoneNumberAsync(request.PhoneNumber, request.Password, request.FirstName, request.LastName);
            if (result.IsFailure) return result.ToIResult(context);

            return await HandleAuthSuccess(result.Value, context, jwtTokenService, refreshTokenService, eventDispatcher, userManager, true, ct);
        });

        // ── Login Endpoints ─────────────────────────────────────────────────────────

        group.MapPost("/login/email", async (
            LoginByEmailRequest request,
            IValidator<LoginByEmailRequest> validator,
            IErrorFactory errorFactory,
            ApplicationUserManager userManager,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            DummyJson.Infrastructure.Events.IntegrationEventDispatcher eventDispatcher,
            HttpContext context,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateToResultAsync(request, errorFactory, ct);
            if (validationResult.IsFailure) return validationResult.ToIResult(context);

            var result = await userManager.LoginByEmailAsync(request.Email, request.Password);
            if (result.IsFailure) return result.ToIResult(context);

            return await HandleAuthSuccess(result.Value, context, jwtTokenService, refreshTokenService, eventDispatcher, userManager, false, ct);
        });

        group.MapPost("/login/username", async (
            LoginByUsernameRequest request,
            IValidator<LoginByUsernameRequest> validator,
            IErrorFactory errorFactory,
            ApplicationUserManager userManager,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            DummyJson.Infrastructure.Events.IntegrationEventDispatcher eventDispatcher,
            HttpContext context,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateToResultAsync(request, errorFactory, ct);
            if (validationResult.IsFailure) return validationResult.ToIResult(context);

            var result = await userManager.LoginByUsernameAsync(request.Username, request.Password);
            if (result.IsFailure) return result.ToIResult(context);

            return await HandleAuthSuccess(result.Value, context, jwtTokenService, refreshTokenService, eventDispatcher, userManager, false, ct);
        });

        group.MapPost("/login/phone", async (
            LoginByPhoneNumberRequest request,
            IValidator<LoginByPhoneNumberRequest> validator,
            IErrorFactory errorFactory,
            ApplicationUserManager userManager,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            DummyJson.Infrastructure.Events.IntegrationEventDispatcher eventDispatcher,
            HttpContext context,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateToResultAsync(request, errorFactory, ct);
            if (validationResult.IsFailure) return validationResult.ToIResult(context);

            var result = await userManager.LoginByPhoneNumberAsync(request.PhoneNumber, request.Password);
            if (result.IsFailure) return result.ToIResult(context);

            return await HandleAuthSuccess(result.Value, context, jwtTokenService, refreshTokenService, eventDispatcher, userManager, false, ct);
        });

        // ── Google Auth ─────────────────────────────────────────────────────────────

        group.MapPost("/google", async (
            GoogleAuthRequest request,
            IValidator<GoogleAuthRequest> validator,
            IErrorFactory errorFactory,
            ApplicationUserManager userManager,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            DummyJson.Infrastructure.Events.IntegrationEventDispatcher eventDispatcher,
            HttpContext context,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateToResultAsync(request, errorFactory, ct);
            if (validationResult.IsFailure) return validationResult.ToIResult(context);

            var result = await userManager.LoginOrRegisterByGoogleAsync(request.GoogleIdToken, request.FirstName, request.LastName);
            if (result.IsFailure) return result.ToIResult(context);

            // We consider it a new registration if they don't have roles or just got created. 
            // HandleAuthSuccess will safely re-publish the event if we tell it it's new, but Google login could be existing.
            // We can treat it as login since the logic is basically identical.
            return await HandleAuthSuccess(result.Value, context, jwtTokenService, refreshTokenService, eventDispatcher, userManager, false, ct);
        });

        // ── Refresh / Logout ────────────────────────────────────────────────────────

        group.MapPost("/refresh", async (
            RefreshTokenRequest request,
            IValidator<RefreshTokenRequest> validator,
            IErrorFactory errorFactory,
            ApplicationUserManager userManager,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            DummyJson.Infrastructure.Events.IntegrationEventDispatcher eventDispatcher,
            HttpContext context,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateToResultAsync(request, errorFactory, ct);
            if (validationResult.IsFailure) return validationResult.ToIResult(context);

            var tokenValidation = await refreshTokenService.ValidateRefreshTokenAsync(request.RefreshToken, ct);
            if (tokenValidation.IsFailure)
                return Result.Failure(CommonErrors.Unauthorized()).ToIResult(context);

            var user = await userManager.FindByIdAsync(tokenValidation.Value.ToString());
            if (user is null)
                return Result.Failure(CommonErrors.Unauthorized()).ToIResult(context);

            await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken, ct);

            return await HandleAuthSuccess(user, context, jwtTokenService, refreshTokenService, eventDispatcher, userManager, false, ct);
        });

        group.MapPost("/logout", async (
            RefreshTokenRequest request,
            IValidator<RefreshTokenRequest> validator,
            IErrorFactory errorFactory,
            IRefreshTokenService refreshTokenService,
            HttpContext context,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateToResultAsync(request, errorFactory, ct);
            if (validationResult.IsFailure) return validationResult.ToIResult(context);

            await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken, ct);

            if (IsWebClient(context))
            {
                context.Response.Cookies.Delete("AccessToken");
                context.Response.Cookies.Delete("RefreshToken");
            }
            return Results.NoContent();
        });
    }
}
