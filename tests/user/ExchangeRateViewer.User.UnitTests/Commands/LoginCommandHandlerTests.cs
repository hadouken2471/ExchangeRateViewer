using ExchangeRateViewer.Shared.Kernel.Exceptions;
using ExchangeRateViewer.Users.Application.Commands;
using ExchangeRateViewer.Users.Application.Services;
using ExchangeRateViewer.Users.Domain.Entities;
using NSubstitute;

namespace ExchangeRateViewer.User.UnitTests.Commands;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ITokenStore _tokenStore = Substitute.For<ITokenStore>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(
            _userRepository, _tokenStore, _passwordHasher, _jwtTokenService);
    }

    [Fact]
    public async Task Login_Success_ReturnsAuthDto()
    {
        var user = Users.Domain.Entities.User.Create("testuser", "hashed");
        _userRepository.GetByNameAsync("testuser", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("password123", "hashed").Returns(true);
        _jwtTokenService.GenerateTokens(user.Id, "testuser")
            .Returns(("access_token", "refresh_token", DateTime.UtcNow.AddMinutes(15)));
        _jwtTokenService.GetRefreshTokenExpiry().Returns(DateTime.UtcNow.AddDays(7));

        var result = await _handler.Handle(new LoginCommand("testuser", "password123"), CancellationToken.None);

        Assert.Equal("access_token", result.AccessToken);
        await _tokenStore.Received(1).StoreRefreshTokenAsync(
            user.Id, "refresh_token", Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Login_UserNotFound_ThrowsValidationException()
    {
        _userRepository.GetByNameAsync("unknown", Arg.Any<CancellationToken>())
            .Returns((Users.Domain.Entities.User?)null);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new LoginCommand("unknown", "password"), CancellationToken.None));
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsValidationException()
    {
        var user = Users.Domain.Entities.User.Create("testuser", "hashed");
        _userRepository.GetByNameAsync("testuser", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("wrong_password", "hashed").Returns(false);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new LoginCommand("testuser", "wrong_password"), CancellationToken.None));
    }
}
