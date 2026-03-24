using ExchangeRateViewer.Finance.Domain.Entities;
using ExchangeRateViewer.Finance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateViewer.Finance.Infrastructure.Repositories;

public sealed class FavoriteCurrencyRepository(FinanceDbContext context) : IFavoriteCurrencyRepository
{
    public async Task<List<FavoriteCurrency>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.FavoriteCurrencies
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FavoriteCurrency favorite, CancellationToken cancellationToken = default)
    {
        await context.FavoriteCurrencies.AddAsync(favorite, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Guid userId, string currencyId, CancellationToken cancellationToken = default)
    {
        var favorite = await context.FavoriteCurrencies
            .FirstOrDefaultAsync(x => x.UserId == userId && x.CurrencyId == currencyId, cancellationToken);

        if (favorite is not null)
        {
            context.FavoriteCurrencies.Remove(favorite);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid userId, string currencyId, CancellationToken cancellationToken = default)
    {
        return await context.FavoriteCurrencies
            .AnyAsync(x => x.UserId == userId && x.CurrencyId == currencyId, cancellationToken);
    }
}
