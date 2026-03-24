using ExchangeRateViewer.Users.Application.Commands;
using ExchangeRateViewer.Users.Application.Services;
using NSubstitute;

namespace ExchangeRateViewer.User.UnitTests.Commands;

public class LogoutCommandHandlerTests
{
    private readonly ITokenStore _tokenStore = Substitute.For<ITokenStore>();
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _handler = new LogoutCommandHandler(_tokenStore);
    }

    [Fact]
    public async Task Logout_Success_RevokesTokenAndRemovesRefreshTokens()
    {
        var userId = Guid.NewGuid();

        var result = await _handler.Handle(new LogoutCommand("test-jti", userId), CancellationToken.None);

        Assert.True(result);
        await _tokenStore.Received(1).RemoveRefreshTokensByUserIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Logout_VerifiesAccessTokenRevoked()
    {
        var userId = Guid.NewGuid();
        var jti = "test-jti-456";

        await _handler.Handle(new LogoutCommand(jti, userId), CancellationToken.None);

        await _tokenStore.Received(1).RevokeAccessTokenAsync(jti, Arg.Any<CancellationToken>());
    }
}
