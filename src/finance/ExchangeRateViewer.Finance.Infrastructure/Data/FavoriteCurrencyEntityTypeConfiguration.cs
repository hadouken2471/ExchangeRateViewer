using ExchangeRateViewer.Finance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangeRateViewer.Finance.Infrastructure.Data;

internal sealed class FavoriteCurrencyEntityTypeConfiguration : IEntityTypeConfiguration<FavoriteCurrency>
{
    public void Configure(EntityTypeBuilder<FavoriteCurrency> builder)
    {
        builder.ToTable("favorite_currencies");
        builder.HasKey(x => new { x.UserId, x.CurrencyId });
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.CurrencyId).HasColumnName("currency_id").HasMaxLength(10);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(x => x.CurrencyId);
    }
}
