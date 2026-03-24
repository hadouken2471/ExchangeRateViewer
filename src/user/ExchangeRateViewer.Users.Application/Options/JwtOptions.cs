namespace ExchangeRateViewer.Users.Application.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Secret { get; init; }
    public string Issuer { get; init; } = "ExchangeRateViewer.Users";
    public string Audience { get; init; } = "ExchangeRateViewer";
    public int AccessTokenExpirationMinutes { get; init; } = 15;
    public int RefreshTokenExpirationDays { get; init; } = 7;
}
