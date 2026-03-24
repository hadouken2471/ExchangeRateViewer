namespace ExchangeRateViewer.Finance.Domain.Entities;

public interface ICurrencyRepository
{
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
}
