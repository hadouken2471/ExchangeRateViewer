namespace ExchangeRateViewer.Shared.Kernel.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entityName, object key)
        : base($"{entityName} с ключом '{key}' не найден") { }
}
