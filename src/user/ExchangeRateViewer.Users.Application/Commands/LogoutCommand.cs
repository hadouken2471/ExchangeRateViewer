using ExchangeRateViewer.Shared.Cqrs;
using ExchangeRateViewer.Users.Application.Services;

namespace ExchangeRateViewer.Users.Application.Commands;

public record LogoutCommand(string Jti, Guid UserId) : ICommand<bool>;

public sealed class LogoutCommandHandler(
    ITokenStore tokenStore) : ICommandHandler<LogoutCommand, bool>
{
    public async Task<bool> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        await tokenStore.RevokeAccessTokenAsync(command.Jti, cancellationToken);
        await tokenStore.RemoveRefreshTokensByUserIdAsync(command.UserId, cancellationToken);
        return true;
    }
}
