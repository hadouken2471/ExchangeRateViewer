using ExchangeRateViewer.Finance.Domain.Entities;
using ExchangeRateViewer.Shared.Cqrs;
using ExchangeRateViewer.Shared.Kernel.Exceptions;

namespace ExchangeRateViewer.Finance.Application.Commands;

public record RemoveFavoriteCommand(Guid UserId, string CurrencyId) : ICommand<bool>;

public sealed class RemoveFavoriteCommandHandler(
    IFavoriteCurrencyRepository favoriteCurrencyRepository) : ICommandHandler<RemoveFavoriteCommand, bool>
{
    public async Task<bool> Handle(RemoveFavoriteCommand command, CancellationToken cancellationToken)
    {
        if (!await favoriteCurrencyRepository.ExistsAsync(command.UserId, command.CurrencyId, cancellationToken))
        {
            throw new NotFoundException($"Валюта '{command.CurrencyId}' не найдена в избранном");
        }

        await favoriteCurrencyRepository.RemoveAsync(command.UserId, command.CurrencyId, cancellationToken);

        return true;
    }
}
