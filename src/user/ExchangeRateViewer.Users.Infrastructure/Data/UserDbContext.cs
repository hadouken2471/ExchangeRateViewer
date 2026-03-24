using ExchangeRateViewer.Users.Domain.Entities;
using ExchangeRateViewer.Users.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateViewer.Users.Infrastructure.Data;

public class UserDbContext : DbContext
{
    public const string Schema = "users";

    public DbSet<User> Users { get; init; }
    public DbSet<RefreshToken> RefreshTokens { get; init; }
    public DbSet<RevokedToken> RevokedTokens { get; init; }

    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);
    }
}
