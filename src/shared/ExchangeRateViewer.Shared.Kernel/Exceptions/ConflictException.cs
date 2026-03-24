namespace ExchangeRateViewer.Shared.Kernel.Exceptions;

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}
