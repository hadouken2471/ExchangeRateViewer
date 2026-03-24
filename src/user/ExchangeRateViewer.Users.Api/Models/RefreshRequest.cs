namespace ExchangeRateViewer.Users.Api.Models;

public record RefreshRequest(string AccessToken, string RefreshToken);
