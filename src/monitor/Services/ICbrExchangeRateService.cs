using ExchangeRateMonitor.Models;

namespace ExchangeRateMonitor.Services;

public interface ICbrExchangeRateService
{
    Task<List<CurrencyRate>> FetchRatesAsync(CancellationToken cancellationToken);
}
