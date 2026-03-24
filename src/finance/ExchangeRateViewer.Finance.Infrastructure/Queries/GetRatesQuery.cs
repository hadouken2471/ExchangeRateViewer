using ExchangeRateViewer.Finance.Application.Models;
using ExchangeRateViewer.Finance.Infrastructure.Data;
using ExchangeRateViewer.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateViewer.Finance.Infrastructure.Queries;

public record GetRatesQuery : IQuery<List<CurrencyDto>>;

public sealed class GetRatesQueryHandler(FinanceDbContext context) : IQueryHandler<GetRatesQuery, List<CurrencyDto>>
{
    public async Task<List<CurrencyDto>> Handle(GetRatesQuery query, CancellationToken cancellationToken)
    {
        return await context.Currencies
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CurrencyDto(x.Id, x.Name, x.Rate))
            .ToListAsync(cancellationToken);
    }
}
