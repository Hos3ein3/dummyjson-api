using System.Security.Cryptography;
using DummyJson.Application.Auth.Services;
using DummyJson.Persistence.Context;
using SharedKernel.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DummyJson.Persistence.Identity;

/// <summary>
/// Entity Framework Core backed refresh token service using ASP.NET Core Identity.
/// </summary>
public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;

    public RefreshTokenService(UserManager<ApplicationUser> userManager, AppDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task StoreRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return;

        var token = new ApplicationUserRefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiryDate = DateTimeOffset.UtcNow.AddDays(7)
        };

        await _dbContext.UserRefreshTokens.AddAsync(token, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Result<Guid>> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _dbContext.UserRefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked && !t.IsUsed, cancellationToken);
        
        if (token is null || token.ExpiryDate < DateTimeOffset.UtcNow)
        {
            return Result.Failure<Guid>(SharedKernel.Results.CommonErrors.Unauthorized());
        }

        // Mark as used
        token.IsUsed = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(token.User.DomainUserId);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _dbContext.UserRefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);
        
        if (token is not null)
        {
            token.IsRevoked = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
        {
            var tokens = await _dbContext.UserRefreshTokens
                .Where(t => t.UserId == user.Id && !t.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
