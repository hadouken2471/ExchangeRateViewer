using ExchangeRateViewer.Shared.Kernel;

namespace ExchangeRateViewer.Finance.Domain.Entities;

public class Currency : Entity<string>
{
    public string Name { get; private set; }
    public decimal Rate { get; private set; }

    protected Currency() { } // EF Core

    public Currency(string id, string name, decimal rate) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Rate = rate;
    }

    public void UpdateRate(string name, decimal rate)
    {
        Name = name;
        Rate = rate;
    }
}
