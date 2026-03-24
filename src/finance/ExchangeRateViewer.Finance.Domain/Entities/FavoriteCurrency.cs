namespace ExchangeRateViewer.Finance.Domain.Entities;

public class FavoriteCurrency
{
    public Guid UserId { get; private set; }
    public string CurrencyId { get; private set; } = string.Empty;

    protected FavoriteCurrency() { } // EF Core

    public FavoriteCurrency(Guid userId, string currencyId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyId);
        UserId = userId;
        CurrencyId = currencyId;
    }
}
