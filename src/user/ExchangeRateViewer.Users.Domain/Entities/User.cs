using ExchangeRateViewer.Shared.Kernel;

namespace ExchangeRateViewer.Users.Domain.Entities;

public class User : Entity<Guid>
{
    public string Name { get; private set; }
    public string Password { get; private set; }

    protected User() { }

    private User(string name, string hashedPassword) : base(Guid.NewGuid())
    {
        Name = name;
        Password = hashedPassword;
    }

    public static User Create(string name, string hashedPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(hashedPassword);
        return new User(name, hashedPassword);
    }
}
