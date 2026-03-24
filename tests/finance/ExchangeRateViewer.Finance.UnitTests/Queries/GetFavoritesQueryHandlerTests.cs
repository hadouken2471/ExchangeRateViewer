using ExchangeRateViewer.Finance.Domain.Entities;
using ExchangeRateViewer.Finance.Infrastructure.Data;
using ExchangeRateViewer.Finance.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateViewer.Finance.UnitTests.Queries;

public class GetFavoritesQueryHandlerTests : IDisposable
{
    private readonly FinanceDbContext _context;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();

    public GetFavoritesQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FinanceDbContext(options);
    }

    [Fact]
    public async Task GetFavorites_ReturnsOnlyUserFavorites()
    {
        var usd = new Currency("USD", "Доллар США", 84m);
        var eur = new Currency("EUR", "Евро", 97m);
        var gbp = new Currency("GBP", "Фунт стерлингов", 112m);
        _context.Currencies.AddRange(usd, eur, gbp);

        _context.FavoriteCurrencies.AddRange(
            new FavoriteCurrency(_userId, "USD"),
            new FavoriteCurrency(_userId, "EUR"),
            new FavoriteCurrency(_otherUserId, "GBP"));
        await _context.SaveChangesAsync();

        var handler = new GetFavoritesQueryHandler(_context);
        var result = await handler.Handle(new GetFavoritesQuery(_userId), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, dto => Assert.Contains(dto.Id, new[] { "USD", "EUR" }));
    }

    [Fact]
    public async Task GetFavorites_NoFavorites_ReturnsEmptyList()
    {
        var handler = new GetFavoritesQueryHandler(_context);
        var result = await handler.Handle(new GetFavoritesQuery(_userId), CancellationToken.None);

        Assert.Empty(result);
    }

    public void Dispose() => _context.Dispose();
}
