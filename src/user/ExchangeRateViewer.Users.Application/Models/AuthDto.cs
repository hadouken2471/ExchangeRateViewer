namespace ExchangeRateViewer.Users.Application.Models;

public record AuthDto(string AccessToken, string RefreshToken, DateTime ExpiresAt);
