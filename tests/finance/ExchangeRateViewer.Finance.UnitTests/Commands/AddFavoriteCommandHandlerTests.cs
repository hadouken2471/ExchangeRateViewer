using ExchangeRateViewer.Finance.Application.Commands;
using ExchangeRateViewer.Finance.Domain.Entities;
using ExchangeRateViewer.Shared.Kernel.Exceptions;
using NSubstitute;

namespace ExchangeRateViewer.Finance.UnitTests.Commands;

public class AddFavoriteCommandHandlerTests
{
    private readonly ICurrencyRepository _currencyRepository = Substitute.For<ICurrencyRepository>();
    private readonly IFavoriteCurrencyRepository _favoriteRepository = Substitute.For<IFavoriteCurrencyRepository>();
    private readonly AddFavoriteCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public AddFavoriteCommandHandlerTests()
    {
        _handler = new AddFavoriteCommandHandler(_currencyRepository, _favoriteRepository);
    }

    [Fact]
    public async Task AddFavorite_Success_AddsFavorite()
    {
        _currencyRepository.ExistsAsync("USD", Arg.Any<CancellationToken>()).Returns(true);
        _favoriteRepository.ExistsAsync(_userId, "USD", Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(new AddFavoriteCommand(_userId, "USD"), CancellationToken.None);

        Assert.True(result);
        await _favoriteRepository.Received(1).AddAsync(
            Arg.Is<FavoriteCurrency>(f => f.UserId == _userId && f.CurrencyId == "USD"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddFavorite_CurrencyNotFound_ThrowsNotFoundException()
    {
        _currencyRepository.ExistsAsync("XXX", Arg.Any<CancellationToken>()).Returns(false);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new AddFavoriteCommand(_userId, "XXX"), CancellationToken.None));
    }

    [Fact]
    public async Task AddFavorite_AlreadyFavorited_ThrowsConflictException()
    {
        _currencyRepository.ExistsAsync("USD", Arg.Any<CancellationToken>()).Returns(true);
        _favoriteRepository.ExistsAsync(_userId, "USD", Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(new AddFavoriteCommand(_userId, "USD"), CancellationToken.None));
    }
}
