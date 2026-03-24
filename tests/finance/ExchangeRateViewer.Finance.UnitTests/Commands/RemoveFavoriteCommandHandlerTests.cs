using ExchangeRateViewer.Finance.Application.Commands;
using ExchangeRateViewer.Finance.Domain.Entities;
using ExchangeRateViewer.Shared.Kernel.Exceptions;
using NSubstitute;

namespace ExchangeRateViewer.Finance.UnitTests.Commands;

public class RemoveFavoriteCommandHandlerTests
{
    private readonly IFavoriteCurrencyRepository _favoriteRepository = Substitute.For<IFavoriteCurrencyRepository>();
    private readonly RemoveFavoriteCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public RemoveFavoriteCommandHandlerTests()
    {
        _handler = new RemoveFavoriteCommandHandler(_favoriteRepository);
    }

    [Fact]
    public async Task RemoveFavorite_Success_RemovesFavorite()
    {
        _favoriteRepository.ExistsAsync(_userId, "USD", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(new RemoveFavoriteCommand(_userId, "USD"), CancellationToken.None);

        Assert.True(result);
        await _favoriteRepository.Received(1).RemoveAsync(_userId, "USD", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveFavorite_NotInFavorites_ThrowsNotFoundException()
    {
        _favoriteRepository.ExistsAsync(_userId, "EUR", Arg.Any<CancellationToken>()).Returns(false);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new RemoveFavoriteCommand(_userId, "EUR"), CancellationToken.None));
    }
}
