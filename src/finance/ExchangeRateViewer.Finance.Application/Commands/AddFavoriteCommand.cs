using ExchangeRateViewer.Finance.Domain.Entities;
using ExchangeRateViewer.Shared.Cqrs;
using ExchangeRateViewer.Shared.Kernel.Exceptions;

namespace ExchangeRateViewer.Finance.Application.Commands;

public record AddFavoriteCommand(Guid UserId, string CurrencyId) : ICommand<bool>;

public sealed class AddFavoriteCommandHandler(
    ICurrencyRepository currencyRepository,
    IFavoriteCurrencyRepository favoriteCurrencyRepository) : ICommandHandler<AddFavoriteCommand, bool>
{
    public async Task<bool> Handle(AddFavoriteCommand command, CancellationToken cancellationToken)
    {
        if (!await currencyRepository.ExistsAsync(command.CurrencyId, cancellationToken))
        {
            throw new NotFoundException($"Валюта с кодом '{command.CurrencyId}' не найдена");
        }

        if (await favoriteCurrencyRepository.ExistsAsync(command.UserId, command.CurrencyId, cancellationToken))
        {
            throw new ConflictException($"Курс валют '{command.CurrencyId}' уже в списке избранных");
        }

        await favoriteCurrencyRepository.AddAsync(
            new FavoriteCurrency(command.UserId, command.CurrencyId), cancellationToken);

        return true;
    }
}
