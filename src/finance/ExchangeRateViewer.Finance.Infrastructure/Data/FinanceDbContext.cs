using ExchangeRateViewer.Finance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateViewer.Finance.Infrastructure.Data;

public class FinanceDbContext : DbContext
{
    public const string Schema = "finance";

    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<FavoriteCurrency> FavoriteCurrencies => Set<FavoriteCurrency>();

    public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceDbContext).Assembly);
    }
}
