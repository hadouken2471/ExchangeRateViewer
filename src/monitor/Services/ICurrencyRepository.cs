using ExchangeRateMonitor.Models;

namespace ExchangeRateMonitor.Services;

public interface ICurrencyRepository
{
    Task SaveRatesAsync(List<CurrencyRate> rates, CancellationToken cancellationToken);
}
