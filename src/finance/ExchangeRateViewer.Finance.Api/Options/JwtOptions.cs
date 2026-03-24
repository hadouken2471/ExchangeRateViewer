namespace ExchangeRateViewer.Finance.Api.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Secret { get; init; }
    public string Issuer { get; init; } = "ExchangeRateViewer.Users";
    public string Audience { get; init; } = "ExchangeRateViewer";
}
