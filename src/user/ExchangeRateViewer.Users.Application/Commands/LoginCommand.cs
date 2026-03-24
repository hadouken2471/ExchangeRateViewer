using ExchangeRateViewer.Shared.Cqrs;
using ExchangeRateViewer.Shared.Kernel.Exceptions;
using ExchangeRateViewer.Users.Application.Models;
using ExchangeRateViewer.Users.Application.Services;
using ExchangeRateViewer.Users.Domain.Entities;

namespace ExchangeRateViewer.Users.Application.Commands;

public record LoginCommand(string Name, string Password) : ICommand<AuthDto>;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    ITokenStore tokenStore,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : ICommandHandler<LoginCommand, AuthDto>
{
    public async Task<AuthDto> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByNameAsync(command.Name, cancellationToken)
                   ?? throw new ValidationException("Неверное имя пользователя или пароль");

        if (!passwordHasher.Verify(command.Password, user.Password))
        {
            throw new ValidationException("Неверное имя пользователя или пароль");
        }

        var (accessToken, refreshToken, expiresAt) = jwtTokenService.GenerateTokens(user.Id, user.Name);
        await tokenStore.StoreRefreshTokenAsync(user.Id, refreshToken, jwtTokenService.GetRefreshTokenExpiry(), cancellationToken);

        return new AuthDto(accessToken, refreshToken, expiresAt);
    }
}
