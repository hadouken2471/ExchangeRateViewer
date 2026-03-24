using ExchangeRateViewer.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangeRateViewer.Users.Infrastructure.Data;

internal sealed class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(255);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Password)
            .HasColumnName("password")
            .HasMaxLength(255)
            .IsRequired(false);
    }
}
