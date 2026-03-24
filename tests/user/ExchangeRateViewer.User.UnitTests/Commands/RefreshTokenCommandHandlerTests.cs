using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ExchangeRateViewer.Shared.Kernel.Exceptions;
using ExchangeRateViewer.Users.Application.Commands;
using ExchangeRateViewer.Users.Application.Services;
using ExchangeRateViewer.Users.Domain.Entities;
using NSubstitute;

namespace ExchangeRateViewer.User.UnitTests.Commands;

public class RefreshTokenCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ITokenStore _tokenStore = Substitute.For<ITokenStore>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly RefreshTokenCommandHandler _handler;

    private readonly Guid _userId = Guid.NewGuid();

    public RefreshTokenCommandHandlerTests()
    {
        _handler = new RefreshTokenCommandHandler(
            _userRepository, _tokenStore, _jwtTokenService);
    }

    private static ClaimsPrincipal CreatePrincipal(string jti, Guid userId)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }

    [Fact]
    public async Task Refresh_Success_ReturnsNewTokenPair()
    {
        var principal = CreatePrincipal("jti-123", _userId);
        _jwtTokenService.ValidateExpiredToken("old_access").Returns(principal);
        _tokenStore.IsAccessTokenRevokedAsync("jti-123", Arg.Any<CancellationToken>()).Returns(false);
        _tokenStore.GetRefreshTokenAsync("old_refresh", Arg.Any<CancellationToken>())
            .Returns((_userId, DateTime.UtcNow.AddDays(1)));
        var user = Users.Domain.Entities.User.Create("testuser", "hashed");
        _userRepository.GetByIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(user);
        _jwtTokenService.GenerateTokens(user.Id, "testuser")
            .Returns(("new_access", "new_refresh", DateTime.UtcNow.AddMinutes(15)));
        _jwtTokenService.GetRefreshTokenExpiry().Returns(DateTime.UtcNow.AddDays(7));

        var result = await _handler.Handle(
            new RefreshTokenCommand("old_access", "old_refresh"), CancellationToken.None);

        Assert.Equal("new_access", result.AccessToken);
        Assert.Equal("new_refresh", result.RefreshToken);
    }

    [Fact]
    public async Task Refresh_InvalidAccessToken_ThrowsValidationException()
    {
        _jwtTokenService.ValidateExpiredToken("invalid").Returns((ClaimsPrincipal?)null);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new RefreshTokenCommand("invalid", "refresh"), CancellationToken.None));
    }

    [Fact]
    public async Task Refresh_TokenRevoked_ThrowsValidationException()
    {
        var principal = CreatePrincipal("revoked-jti", _userId);
        _jwtTokenService.ValidateExpiredToken("access").Returns(principal);
        _tokenStore.IsAccessTokenRevokedAsync("revoked-jti", Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new RefreshTokenCommand("access", "refresh"), CancellationToken.None));
    }

    [Fact]
    public async Task Refresh_ExpiredRefreshToken_ThrowsValidationException()
    {
        var principal = CreatePrincipal("jti-123", _userId);
        _jwtTokenService.ValidateExpiredToken("access").Returns(principal);
        _tokenStore.IsAccessTokenRevokedAsync("jti-123", Arg.Any<CancellationToken>()).Returns(false);
        _tokenStore.GetRefreshTokenAsync("expired_refresh", Arg.Any<CancellationToken>())
            .Returns((_userId, DateTime.UtcNow.AddDays(-1)));

        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new RefreshTokenCommand("access", "expired_refresh"), CancellationToken.None));
    }
}
