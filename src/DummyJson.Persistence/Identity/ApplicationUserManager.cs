using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using DummyJson.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Results;

using DummyJson.Domain.Users;

namespace DummyJson.Persistence.Identity;

/// <summary>
/// Custom UserManager to support locating users by Username, Email, or PhoneNumber
/// and perfectly encapsulating all authentication and registration flows.
/// </summary>
public sealed class ApplicationUserManager : UserManager<ApplicationUser>
{
    public ApplicationUserManager(
        IUserStore<ApplicationUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<ApplicationUser>> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
    }

    // ── Helper: Uniqueness Checks ─────────────────────────────────────────────
    
    private async Task<Result> CheckEmailUniqueAsync(string email)
    {
        if (await Users.AnyAsync(u => u.Email == email))
            return Result.Failure(Error.Failure("Auth.DuplicateEmail", "Email is already taken."));
        return Result.Success();
    }

    private async Task<Result> CheckUsernameUniqueAsync(string username)
    {
        if (await Users.AnyAsync(u => u.UserName == username))
            return Result.Failure(Error.Failure("Auth.DuplicateUsername", "Username is already taken."));
        return Result.Success();
    }

    private async Task<Result> CheckPhoneUniqueAsync(string phoneNumber)
    {
        if (await Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
            return Result.Failure(Error.Failure("Auth.DuplicatePhone", "Phone number is already taken."));
        return Result.Success();
    }

    // ── Registration Methods ──────────────────────────────────────────────────

    public async Task<Result<ApplicationUser>> RegisterByEmailAsync(string email, string password, string firstName, string lastName)
    {
        var uniqueCheck = await CheckEmailUniqueAsync(email);
        if (uniqueCheck.IsFailure) return Result.Failure<ApplicationUser>(uniqueCheck.Error);

        var userResult = ApplicationUser.Create(firstName, lastName, email, email, "");
        if (userResult.IsFailure) return Result.Failure<ApplicationUser>(userResult.Error);
        var user = userResult.Value;

        return await CreateUserAndRoleAsync(user, password);
    }

    public async Task<Result<ApplicationUser>> RegisterByUsernameAsync(string username, string password, string firstName, string lastName)
    {
        var uniqueCheck = await CheckUsernameUniqueAsync(username);
        if (uniqueCheck.IsFailure) return Result.Failure<ApplicationUser>(uniqueCheck.Error);

        var userResult = ApplicationUser.Create(firstName, lastName, username, "", "");
        if (userResult.IsFailure) return Result.Failure<ApplicationUser>(userResult.Error);
        var user = userResult.Value;

        return await CreateUserAndRoleAsync(user, password);
    }

    public async Task<Result<ApplicationUser>> RegisterByPhoneNumberAsync(string phoneNumber, string password, string firstName, string lastName)
    {
        var uniqueCheck = await CheckPhoneUniqueAsync(phoneNumber);
        if (uniqueCheck.IsFailure) return Result.Failure<ApplicationUser>(uniqueCheck.Error);

        var userResult = ApplicationUser.Create(firstName, lastName, phoneNumber, "", phoneNumber);
        if (userResult.IsFailure) return Result.Failure<ApplicationUser>(userResult.Error);
        var user = userResult.Value;

        return await CreateUserAndRoleAsync(user, password);
    }

    private async Task<Result<ApplicationUser>> CreateUserAndRoleAsync(ApplicationUser user, string? password = null)
    {
        var identityResult = password is not null 
            ? await CreateAsync(user, password) 
            : await CreateAsync(user);

        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors.Select(e => Error.Failure(e.Code, e.Description)).ToList();
            return Result.Failure<ApplicationUser>(Error.ValidationSummary("Auth.RegisterFailed", "User registration failed", errors));
        }

        await AddToRoleAsync(user, "user");
        return Result.Success(user);
    }

    // ── Login Methods ─────────────────────────────────────────────────────────

    public async Task<Result<ApplicationUser>> LoginByEmailAsync(string email, string password)
    {
        var user = await FindByEmailAsync(email);
        return await ValidateUserPasswordAsync(user, password);
    }

    public async Task<Result<ApplicationUser>> LoginByUsernameAsync(string username, string password)
    {
        var user = await FindByNameAsync(username);
        return await ValidateUserPasswordAsync(user, password);
    }

    public async Task<Result<ApplicationUser>> LoginByPhoneNumberAsync(string phoneNumber, string password)
    {
        var user = await Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        return await ValidateUserPasswordAsync(user, password);
    }

    private async Task<Result<ApplicationUser>> ValidateUserPasswordAsync(ApplicationUser? user, string password)
    {
        if (user is null || !await CheckPasswordAsync(user, password))
            return Result.Failure<ApplicationUser>(Error.Failure("Auth.Unauthorized", "Invalid credentials."));

        return Result.Success(user);
    }

    // ── Google OAuth ──────────────────────────────────────────────────────────

    public async Task<Result<ApplicationUser>> LoginOrRegisterByGoogleAsync(string googleIdToken, string? firstName = null, string? lastName = null)
    {
        // Decode the JWT token (in production, use Google.Apis.Auth to verify signature and audience)
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(googleIdToken))
            return Result.Failure<ApplicationUser>(Error.Failure("Auth.InvalidGoogleToken", "Invalid Google ID token format."));

        var token = handler.ReadJwtToken(googleIdToken);
        var email = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var googleSubjectId = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        var tokenFirstName = token.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value ?? firstName ?? "GoogleUser";
        var tokenLastName = token.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value ?? lastName ?? "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleSubjectId))
            return Result.Failure<ApplicationUser>(Error.Failure("Auth.InvalidGooglePayload", "Missing email or subject in Google token."));

        // Check if user already exists
        var user = await FindByEmailAsync(email);
        
        if (user is not null)
        {
            // Login successful
            return Result.Success(user);
        }

        // Register new user
        var uniqueCheck = await CheckEmailUniqueAsync(email);
        if (uniqueCheck.IsFailure) return Result.Failure<ApplicationUser>(uniqueCheck.Error);

        var userResult = ApplicationUser.Create(tokenFirstName, tokenLastName, email, email, "");
        if (userResult.IsFailure) return Result.Failure<ApplicationUser>(userResult.Error);
        
        var newUser = userResult.Value;
        newUser.EmailConfirmed = true;

        return await CreateUserAndRoleAsync(newUser);
    }
}
