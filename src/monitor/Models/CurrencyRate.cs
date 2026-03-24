namespace ExchangeRateMonitor.Models;

public class CurrencyRate
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public decimal Rate { get; init; }
}
