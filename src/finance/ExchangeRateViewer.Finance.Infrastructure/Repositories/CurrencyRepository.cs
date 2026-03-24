using ExchangeRateViewer.Finance.Domain.Entities;
using ExchangeRateViewer.Finance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateViewer.Finance.Infrastructure.Repositories;

public sealed class CurrencyRepository(FinanceDbContext context) : ICurrencyRepository
{
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await context.Currencies
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
}
