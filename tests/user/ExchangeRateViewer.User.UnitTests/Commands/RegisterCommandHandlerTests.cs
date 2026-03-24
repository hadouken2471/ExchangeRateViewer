using ExchangeRateViewer.Shared.Kernel.Exceptions;
using ExchangeRateViewer.Users.Application.Commands;
using ExchangeRateViewer.Users.Application.Services;
using ExchangeRateViewer.Users.Domain.Entities;
using NSubstitute;

namespace ExchangeRateViewer.User.UnitTests.Commands;

public class RegisterCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ITokenStore _tokenStore = Substitute.For<ITokenStore>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(
            _userRepository, _tokenStore, _passwordHasher, _jwtTokenService);
    }

    [Fact]
    public async Task Register_Success_ReturnsAuthDto()
    {
        _userRepository.GetByNameAsync("testuser", Arg.Any<CancellationToken>())
            .Returns((Users.Domain.Entities.User?)null);
        _userRepository.AddAsync(Arg.Any<Users.Domain.Entities.User>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Users.Domain.Entities.User>());
        _passwordHasher.Hash("password123").Returns("hashed_password");
        _jwtTokenService.GenerateTokens(Arg.Any<Guid>(), "testuser")
            .Returns(("access_token", "refresh_token", DateTime.UtcNow.AddMinutes(15)));
        _jwtTokenService.GetRefreshTokenExpiry().Returns(DateTime.UtcNow.AddDays(7));

        var result = await _handler.Handle(new RegisterCommand("testuser", "password123"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("access_token", result.AccessToken);
        await _tokenStore.Received(1).StoreRefreshTokenAsync(
            Arg.Any<Guid>(), "refresh_token", Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_UserAlreadyExists_ThrowsConflictException()
    {
        var existingUser = Users.Domain.Entities.User.Create("testuser", "hashed");
        _userRepository.GetByNameAsync("testuser", Arg.Any<CancellationToken>()).Returns(existingUser);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(new RegisterCommand("testuser", "password123"), CancellationToken.None));
    }

    [Fact]
    public async Task Register_PasswordIsHashed()
    {
        _userRepository.GetByNameAsync("testuser", Arg.Any<CancellationToken>())
            .Returns((Users.Domain.Entities.User?)null);
        _userRepository.AddAsync(Arg.Any<Users.Domain.Entities.User>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Users.Domain.Entities.User>());
        _passwordHasher.Hash("password123").Returns("hashed_password");
        _jwtTokenService.GenerateTokens(Arg.Any<Guid>(), "testuser")
            .Returns(("token", "refresh", DateTime.UtcNow));
        _jwtTokenService.GetRefreshTokenExpiry().Returns(DateTime.UtcNow.AddDays(7));

        await _handler.Handle(new RegisterCommand("testuser", "password123"), CancellationToken.None);

        _passwordHasher.Received(1).Hash("password123");
        await _userRepository.Received(1).AddAsync(
            Arg.Is<Users.Domain.Entities.User>(u => u.Password == "hashed_password"),
            Arg.Any<CancellationToken>());
    }
}
