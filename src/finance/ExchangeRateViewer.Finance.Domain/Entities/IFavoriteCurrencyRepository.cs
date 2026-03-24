namespace ExchangeRateViewer.Finance.Domain.Entities;

public interface IFavoriteCurrencyRepository
{
    Task<List<FavoriteCurrency>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(FavoriteCurrency favorite, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid userId, string currencyId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, string currencyId, CancellationToken cancellationToken = default);
}
