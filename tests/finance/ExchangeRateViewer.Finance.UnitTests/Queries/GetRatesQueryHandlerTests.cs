using ExchangeRateViewer.Finance.Domain.Entities;
using ExchangeRateViewer.Finance.Infrastructure.Data;
using ExchangeRateViewer.Finance.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateViewer.Finance.UnitTests.Queries;

public class GetRatesQueryHandlerTests : IDisposable
{
    private readonly FinanceDbContext _context;

    public GetRatesQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FinanceDbContext(options);
    }

    [Fact]
    public async Task GetRates_ReturnsSortedCurrencies()
    {
        _context.Currencies.AddRange(
            new Currency("USD", "Доллар США", 84m),
            new Currency("EUR", "Евро", 97m),
            new Currency("AUD", "Австралийский доллар", 59m));
        await _context.SaveChangesAsync();

        var handler = new GetRatesQueryHandler(_context);
        var result = await handler.Handle(new GetRatesQuery(), CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal("AUD", result[0].Id); // Австралийский доллар по алфавиту первый
        Assert.Equal("Доллар США", result[1].Name);
        Assert.Equal(97m, result[2].Rate);
    }

    [Fact]
    public async Task GetRates_EmptyDb_ReturnsEmptyList()
    {
        var handler = new GetRatesQueryHandler(_context);
        var result = await handler.Handle(new GetRatesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    public void Dispose() => _context.Dispose();
}
