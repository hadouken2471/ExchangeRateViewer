using ExchangeRateViewer.Finance.Application.Models;
using ExchangeRateViewer.Finance.Infrastructure.Data;
using ExchangeRateViewer.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateViewer.Finance.Infrastructure.Queries;

public record GetFavoritesQuery(Guid UserId) : IQuery<List<CurrencyDto>>;

public sealed class GetFavoritesQueryHandler(FinanceDbContext context) : IQueryHandler<GetFavoritesQuery, List<CurrencyDto>>
{
    public async Task<List<CurrencyDto>> Handle(GetFavoritesQuery query, CancellationToken cancellationToken)
    {
        return await context.FavoriteCurrencies
            .AsNoTracking()
            .Where(f => f.UserId == query.UserId)
            .Join(context.Currencies,
                f => f.CurrencyId,
                c => c.Id,
                (f, c) => new CurrencyDto(c.Id, c.Name, c.Rate))
            .ToListAsync(cancellationToken);
    }
}
