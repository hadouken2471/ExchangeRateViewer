namespace ExchangeRateViewer.Users.Application.Services;

public interface ITokenStore
{
    Task StoreRefreshTokenAsync(Guid userId, string token, DateTime expiresAt, CancellationToken cancellationToken = default);
    Task<(Guid UserId, DateTime ExpiresAt)?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RemoveRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RemoveRefreshTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RevokeAccessTokenAsync(string jti, CancellationToken cancellationToken = default);
    Task<bool> IsAccessTokenRevokedAsync(string jti, CancellationToken cancellationToken = default);
}
