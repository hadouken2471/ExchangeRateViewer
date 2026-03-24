using ExchangeRateViewer.Shared.Cqrs;
using ExchangeRateViewer.Shared.Kernel.Exceptions;
using ExchangeRateViewer.Users.Application.Models;
using ExchangeRateViewer.Users.Application.Services;
using ExchangeRateViewer.Users.Domain.Entities;

namespace ExchangeRateViewer.Users.Application.Commands;

public record RegisterCommand(string Name, string Password) : ICommand<AuthDto>;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    ITokenStore tokenStore,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : ICommandHandler<RegisterCommand, AuthDto>
{
    public async Task<AuthDto> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByNameAsync(command.Name, cancellationToken);
        if (existingUser is not null)
        {
            throw new ConflictException($"Пользователь '{command.Name}' уже существует");
        }

        var user = User.Create(command.Name, passwordHasher.Hash(command.Password));
        await userRepository.AddAsync(user, cancellationToken);

        var (accessToken, refreshToken, expiresAt) = jwtTokenService.GenerateTokens(user.Id, user.Name);
        await tokenStore.StoreRefreshTokenAsync(user.Id, refreshToken, jwtTokenService.GetRefreshTokenExpiry(), cancellationToken);

        return new AuthDto(accessToken, refreshToken, expiresAt);
    }
}
