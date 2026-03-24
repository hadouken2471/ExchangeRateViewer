using ExchangeRateViewer.Users.Application.Services;
using ExchangeRateViewer.Users.Infrastructure.Data;
using ExchangeRateViewer.Users.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateViewer.Users.Infrastructure.Services;

public class TokenStore(UserDbContext context) : ITokenStore
{
    public async Task StoreRefreshTokenAsync(Guid userId, string token, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        await context.RefreshTokens.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(Guid UserId, DateTime ExpiresAt)?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var entity = await context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);

        return entity is null ? null : (entity.UserId, entity.ExpiresAt);
    }

    public async Task RemoveRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var entity = await context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);

        if (entity is not null)
        {
            context.RefreshTokens.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemoveRefreshTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = context.RefreshTokens.Where(x => x.UserId == userId);
        context.RefreshTokens.RemoveRange(tokens);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAccessTokenAsync(string jti, CancellationToken cancellationToken = default)
    {
        await context.RevokedTokens.AddAsync(new RevokedToken
        {
            Id = Guid.NewGuid(),
            Jti = jti,
            RevokedAt = DateTime.UtcNow
        }, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsAccessTokenRevokedAsync(string jti, CancellationToken cancellationToken = default)
    {
        return await context.RevokedTokens
            .AnyAsync(x => x.Jti == jti, cancellationToken);
    }
}
