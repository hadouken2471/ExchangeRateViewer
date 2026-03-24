namespace ExchangeRateViewer.Users.Infrastructure.Data.Entities;

public class RevokedToken
{
    public Guid Id { get; set; }
    public string Jti { get; set; } = string.Empty;
    public DateTime RevokedAt { get; set; }
}
