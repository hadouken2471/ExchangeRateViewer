using ExchangeRateViewer.Users.Domain.Entities;
using ExchangeRateViewer.Users.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateViewer.Users.Infrastructure.Repositories;

public sealed class UserRepository(UserDbContext context) : IUserRepository
{
    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var result = await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return result.Entity;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<User?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }
}
