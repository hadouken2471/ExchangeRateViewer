using System.Security.Claims;

namespace ExchangeRateViewer.Users.Application.Services;

public interface IJwtTokenService
{
    (string AccessToken, string RefreshToken, DateTime ExpiresAt) GenerateTokens(Guid userId, string userName);
    ClaimsPrincipal? ValidateExpiredToken(string token);
    DateTime GetRefreshTokenExpiry();
}
