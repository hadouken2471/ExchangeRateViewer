using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ExchangeRateViewer.Shared.Cqrs;
using ExchangeRateViewer.Shared.Kernel.Exceptions;
using ExchangeRateViewer.Users.Application.Models;
using ExchangeRateViewer.Users.Application.Services;
using ExchangeRateViewer.Users.Domain.Entities;

namespace ExchangeRateViewer.Users.Application.Commands;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : ICommand<AuthDto>;

public sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    ITokenStore tokenStore,
    IJwtTokenService jwtTokenService) : ICommandHandler<RefreshTokenCommand, AuthDto>
{
    public async Task<AuthDto> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var principal = jwtTokenService.ValidateExpiredToken(command.AccessToken)
                        ?? throw new ValidationException("Невалидный access token");

        var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value
                  ?? throw new ValidationException("Токен не содержит JTI");

        if (await tokenStore.IsAccessTokenRevokedAsync(jti, cancellationToken))
        {
            throw new ValidationException("Токен был отозван");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                          ?? throw new ValidationException("Токен не содержит идентификатор пользователя");

        var userId = Guid.Parse(userIdClaim);

        var stored = await tokenStore.GetRefreshTokenAsync(command.RefreshToken, cancellationToken)
                     ?? throw new ValidationException("Невалидный refresh token");

        if (stored.UserId != userId)
        {
            throw new ValidationException("Refresh token не принадлежит пользователю");
        }

        if (stored.ExpiresAt < DateTime.UtcNow)
        {
            throw new ValidationException("Срок действия refresh token истёк");
        }

        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
                   ?? throw new NotFoundException("Пользователь", userId);

        await tokenStore.RemoveRefreshTokenAsync(command.RefreshToken, cancellationToken);

        var (accessToken, refreshToken, expiresAt) = jwtTokenService.GenerateTokens(user.Id, user.Name);
        await tokenStore.StoreRefreshTokenAsync(user.Id, refreshToken, jwtTokenService.GetRefreshTokenExpiry(), cancellationToken);

        return new AuthDto(accessToken, refreshToken, expiresAt);
    }
}
